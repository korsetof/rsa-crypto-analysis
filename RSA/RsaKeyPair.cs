using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    internal class RsaKeyPair
    {
        public long P { get; set; }
        public long Q { get; set; }
        public long N { get; set; }
        public long Phi { get; set; }
        public long E { get; set; }
        public long D { get; set; }

        public (long e, long n) PublicKey => (E, N);
        public (long d, long n) PrivateKey => (D, N);

        public override string ToString()
        {
            return $"p={P}, q={Q}, n={N}, φ(n)={Phi}, e={E}, d={D}";
        }
    }
}
