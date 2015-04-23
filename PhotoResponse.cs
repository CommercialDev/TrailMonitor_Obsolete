using System;
using Microsoft.SPOT;

namespace GadgeteerCamera4
{
    public class PhotoResponse
    {
        public string sasUrl { get; set; }
        public string header { get; set; }
        public string photoId { get; set; }
        public string expiry { get; set; }
    }
}
