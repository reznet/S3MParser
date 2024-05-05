using System.Diagnostics;

namespace S3mToMidi.LilyPond
{
    internal class Ottava
    {
        private readonly int mb;
        private readonly int vb;
        private readonly int va;
        private readonly int ma;

        public Ottava(int mb, int vb, int va, int ma)
        {
            this.mb = mb;
            this.vb = vb;
            this.va = va;
            this.ma = ma;
        }

        public int GetOttava(int pitch)
        {
            Debug.Assert(0 < pitch);
            int newOttava = 0;
            if (pitch < mb)
            {
                newOttava = -2;
            }
            else if (pitch < vb)
            {
                newOttava = -1;
            }
            else if (ma < pitch)
            {
                newOttava = 2;
            }
            else if (va < pitch)
            {
                newOttava = 1;
            }

            return newOttava;
        }
    }
}