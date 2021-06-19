using Blazorise;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HashMash.Shared
{
    /*
     *  The main class that will handle the "mashing" (pathetic hashing) of some input text from the user in the Masher.razor componment.
     *  The class will store a total count for every click the user has made to calculate their erspective level, which is also stored.
     *  As the user progresses through levels, the hashing will get more complex.
     */

    public partial class Masher
    {
        //private MD5 _md5Hash;  // An MD5 object for MD5 hash calculation, unused as its broken
        private SHA256 _sha256Hash = SHA256.Create();  // A SHA256 object for SHA256 hash calculation

        private int _bitShift { get; set; }
        private int _charOffset { get; set; }
        private int _modN { get; set; } = 1;  // Default 1 (no div by 0)

        private string _inputValue { get; set; } = "";
        private bool _charOffsetEnabled { get; set; }
        private bool _bitShiftEnabled { get; set; }
        private bool _modNEnabled { get; set; }
        private bool _sha256Enabled { get; set; }

        private int _max_table_chars;  // max number of chars to show on the table; length of input or 8, whichever is less

        private Validations _validations;

        // Check session storage for an existing state, set fields if one exists
        protected override async Task OnInitializedAsync()
        {

            // Test input value for a stored session
            if (await sessionStorage.ContainKeyAsync("masher"))
            {
                string jsonExisting = await sessionStorage.GetItemAsync<string>("masher");
                Masher existing = JsonSerializer.Deserialize<Masher>(jsonExisting);

                _inputValue = existing._inputValue;
                _charOffset = existing._charOffset;
                _bitShift = existing._bitShift;
                _modN = existing._modN;
                _charOffsetEnabled = existing._charOffsetEnabled;
                _bitShiftEnabled = existing._bitShiftEnabled;
                _modNEnabled = existing._modNEnabled;
                _sha256Enabled = existing._sha256Enabled;
            }
            _max_table_chars = Math.Min(_inputValue.Length, 8);    // max number of chars to show on the table; length of input or 8, whichever is less
        }

        // To reflect changes in user input, as user changes the input
        private void InputChanged(string value)
        {

            _max_table_chars = Math.Min(value.Length, 8);

            _inputValue = value;

            _validations.ValidateAll();

            StoreState();
        }

        // Reset state (incl. sessionStorage) but keeps input intact
        private void ResetState()
        {
            // Maybe show warning popup here before resetting

            //reset();    // Reset the current masher, excluding user test input


        }

        // Store the state to session storage
        private async void StoreState()
        {
            _validations.ValidateAll();
            string serialized = JsonSerializer.Serialize(this);
            await sessionStorage.SetItemAsync<string>("masher", serialized);

        }

        private string GetHash(HashAlgorithm hashAlgorithm, string input)
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

        private string MashInput()
        {
            string mashed = "";
            string step1 = "";
            string potentialHash = _inputValue;
            foreach (char ch in potentialHash)
            {
                int c = ch; // need to copy current ch in foreach to manipulate

                if (_charOffsetEnabled) c += _charOffset;
                if (_bitShiftEnabled) c <<= _bitShift;
                if (_modNEnabled) c %= _modN;
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
            if (_sha256Enabled)
            {
                mashed = GetHash(_sha256Hash, mashed);
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

        private string MashCh(int c, int b)
        {
            if (_charOffsetEnabled) c += _charOffset;
            if (_bitShiftEnabled) c <<= _bitShift;
            if (_modNEnabled) c %= _modN;
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
