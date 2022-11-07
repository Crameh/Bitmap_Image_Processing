using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI
{
    class Pixel
    {
        byte r;
        byte g;
        byte b;
        public byte R
        {
            get { return r; }
            set { this.r = value; }
        }
        public byte G
        {
            get { return g; }
            set { this.g = value; }
        }
        public byte B
        {
            get { return b; }
            set { this.b = value; }
        }
        public Pixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
}
