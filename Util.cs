using System.Text;

namespace Seven
{
    internal class Util
    {
        public static int FromAlgebraicToIndex(string algebraic)
        {
            algebraic = algebraic.ToLower();
            return (8 * (algebraic[1] - '0')) - (algebraic[0] - 'a') - 1;
        }

        public static string FromIndexToAlgebraic(int index)
        {
            return $"{(char)('h' - (index % 8))}{1 + index / 8}";
        }

        public static bool IsPieceAtIndex(ulong bitboard, int index)
        {
            return index >= 0 && index <= 63 && ((bitboard >> index) & 1UL) == 1UL;
        }

        public static void ClearBit(int index, ref ulong bitboard)
        {
            bitboard &= ~(1UL << index);
        }

        public static void SetBit(int index, ref ulong bitboard)
        {
            bitboard |= 1UL << index;
        }

        public static ulong ReverseBits(ulong n)
        {
            /*
             * Algorithm explains: 
             * 0x0x5555555555555555 = 01010101... in binary
             * The left part: (n >> 1) & 0x0x5555555555555555 move all the bit 1 in odd index to the right
             * The right part: (n & 0x0x5555555555555555) << 1 marks all the bit 1 in the even index (n & 0x0x5555555555555555), the move
             * all those bit to the left
             * Over all, this will swap the bit in pair: (0, 1) -> (1, 0); (2, 3) -> (3, 2); ...
             * The other lines are the same, they swap bit it group
             */

            n = (n >> 1) & 0x5555555555555555UL | (n & 0x5555555555555555UL) << 1; //Swap 1 bit in pair (0, 1) -> (1, 0)
            n = (n >> 2) & 0x3333333333333333UL | (n & 0x3333333333333333UL) << 2; //Swap 2 bit in pair (01, 10) -> (10, 01)
            n = (n >> 4) & 0x0F0F0F0F0F0F0F0FUL | (n & 0x0F0F0F0F0F0F0F0FUL) << 4; //Swap 4 bit in pair (0111, 1010) -> (1010, 0111)
            n = (n >> 8) & 0x00FF00FF00FF00FFUL | (n & 0x00FF00FF00FF00FFUL) << 8; //Swap 8 bit in pair
            n = (n >> 16) & 0x0000FFFF0000FFFFUL | (n & 0x0000FFFF0000FFFFUL) << 16; //Swap 16 bit in pair 
            n = (n >> 32) | (n << 32); //Swap 32 bit in pair 
            return n;
        }

        public static bool IsAtSameRank(int index1, int index2)
        {
            return index1 / 8 == index2 / 8;
        }

        public static bool IsAtSameFile(int index1, int index2)
        {
            return index1 % 8 == index2 % 8;
        }

        public static bool IsAtSameDiagonal(int index1, int index2)
        {
            return (index1 / 8 + index1 % 8) == (index2 / 8 + index2 % 8);
        }

        public static bool IsAtSameAntiDiagonal(int index1, int index2)
        {
            return (index1 / 8 - index1 % 8) == (index2 / 8 - index2 % 8);
        }

        public static ulong CalculateRayAttackLine(int min, int max, int direction)
        {
            ulong rayline = 0;

            switch (direction)
            {
                case Constants.RANK:
                    rayline = Constants.RANK_MASK[min / 8];
                    break;
                case Constants.FILE:
                    rayline = Constants.FILE_MASK[7 - min % 8];
                    break;
                case Constants.DIAGONAL:
                    rayline = Constants.DIAGONAL_MASK[14 - (min / 8 + min % 8)];
                    break;
                case Constants.ANTI_DIAGONAL:
                    rayline = Constants.ANTI_DIAGONAL_MASK[7 - (min / 8 - min % 8)];
                    break;
            }

            return rayline & ((1UL << max) - 1) & ~((1UL << (min + 1)) - 1);
        }

        public static void PrintBitboard(ulong bitboard)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < 64; i++)
            {
                sb.Insert(0, (bitboard & 1UL));
                if (i % 8 == 7)
                {
                    sb.Insert(0, '\n');
                }
                bitboard >>= 1;
            }

            Console.WriteLine(sb.ToString());
        }

        public static int Min(int a, int b) => a <= b ? a : b;

        public static int Max(int a, int b) => a >= b ? a : b;

        public static int Abs(int a) => a < 0 ? -a : a;

        public static ulong FlipVertical(ulong bitboard)
        {
            return (bitboard << 56) |
                   ((bitboard << 40) & Constants.RANK_MASK[6]) |
                   ((bitboard << 24) & Constants.RANK_MASK[5]) |
                   ((bitboard << 8) & Constants.RANK_MASK[4]) |
                   ((bitboard >> 8) & Constants.RANK_MASK[3]) |
                   ((bitboard >> 24) & Constants.RANK_MASK[2]) |
                   ((bitboard >> 40) & Constants.RANK_MASK[1]) |
                   (bitboard >> 56);
        }

        public static int FlipIndexVertical(int index)
        {
            return 8 * (7 - index / 8) + index % 8;
        }

        public static int GetPieceAtIndex(int index, Chess chess)
        {
            for (int i = 0; i < chess.Boards.Length; i++)
            {
                if (IsPieceAtIndex(chess.Boards[i], index)) return i;
            }
            return -1;
        }
    }
}
