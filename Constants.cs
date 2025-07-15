namespace Seven
{
    internal class Constants
    {
        #region Direction types
        public const int RANK = 1;
        public const int FILE = 8;
        public const int DIAGONAL = 7;
        public const int ANTI_DIAGONAL = 9;
        #endregion

        #region Colors
        public const int WHITE = 8;
        public const int BLACK = 16;
        #endregion

        #region Castling rights
        public const int WHITE_KING_SIDE = 8;
        public const int WHITE_QUEEN_SIDE = 4;
        public const int BLACK_KING_SIDE = 2;
        public const int BLACK_QUEEN_SIDE = 1;
        #endregion

        #region Piece constants
        public const int WHITE_PAWN = 0;
        public const int WHITE_ROOK = 1;
        public const int WHITE_KNIGHT = 2;
        public const int WHITE_BISHOP = 3;
        public const int WHITE_QUEEN = 4;
        public const int WHITE_KING = 5;
        public const int BLACK_PAWN = 6;
        public const int BLACK_ROOK = 7;
        public const int BLACK_KNIGHT = 8;
        public const int BLACK_BISHOP = 9;
        public const int BLACK_QUEEN = 10;
        public const int BLACK_KING = 11;
        #endregion

        // RANK masks
        public static readonly ulong[] RANK_MASK =
        [
            0xFFUL, 0xFF00UL, 0xFF0000UL, 0xFF000000UL,
            0xFF00000000UL, 0xFF0000000000UL, 0xFF000000000000UL, 0xFF00000000000000UL
        ];

        // FILE masks
        public static readonly ulong[] FILE_MASK =
        [
            0x8080808080808080UL, 0x4040404040404040UL, 0x2020202020202020UL, 0x1010101010101010UL,
            0x0808080808080808UL, 0x0404040404040404UL, 0x0202020202020202UL, 0x0101010101010101UL
        ];

        // DIAGONAL masks
        public static readonly ulong[] DIAGONAL_MASK =
        [
            0x8000000000000000UL, 0x4080000000000000UL, 0x2040800000000000UL, 0x1020408000000000UL,
            0x0810204080000000UL, 0x0408102040800000UL, 0x0204081020408000UL, 0x0102040810204080UL,
            0x0001020408102040UL, 0x0000010204081020UL, 0x0000000102040810UL, 0x0000000001020408UL,
            0x0000000000010204UL, 0x0000000000000102UL, 0x0000000000000001UL
        ];

        // ANTI-DIAGONAL masks
        public static readonly ulong[] ANTI_DIAGONAL_MASK =
        [
            0x0100000000000000UL, 0x0201000000000000UL, 0x0402010000000000UL, 0x0804020100000000UL,
            0x1008040201000000UL, 0x2010080402010000UL, 0x4020100804020100UL, 0x8040201008040201UL,
            0x0080402010080402UL, 0x0000804020100804UL, 0x0000008040201008UL, 0x0000000080402010UL,
            0x0000000000804020UL, 0x0000000000008040UL, 0x0000000000000080UL
        ];

        // KNIGHT attack masks
        public static readonly ulong[] KNIGHT_ATTACK =
        [
            0x20400UL, 0x50800UL, 0xA1100UL, 0x142200UL, 0x284400UL, 0x508800UL, 0xA01000UL, 0x402000UL,
            0x2040004UL, 0x5080008UL, 0xA110011UL, 0x14220022UL, 0x28440044UL, 0x50880088UL, 0xA0100010UL, 0x40200020UL,
            0x204000402UL, 0x508000805UL, 0xA1100110AUL, 0x1422002214UL, 0x2844004428UL, 0x5088008850UL, 0xA0100010A0UL, 0x4020002040UL,
            0x20400040200UL, 0x50800080500UL, 0xA1100110A00UL, 0x142200221400UL, 0x284400442800UL, 0x508800885000UL, 0xA0100010A000UL, 0x402000204000UL,
            0x2040004020000UL, 0x5080008050000UL, 0xA1100110A0000UL, 0x14220022140000UL, 0x28440044280000UL, 0x50880088500000UL, 0xA0100010A00000UL, 0x40200020400000UL,
            0x204000402000000UL, 0x508000805000000UL, 0xA1100110A000000UL, 0x1422002214000000UL, 0x2844004428000000UL, 0x5088008850000000UL, 0xA0100010A0000000UL, 0x4020002040000000UL,
            0x400040200000000UL, 0x800080500000000UL, 0x1100110A00000000UL, 0x2200221400000000UL, 0x4400442800000000UL, 0x8800885000000000UL, 0x100010A000000000UL, 0x2000204000000000UL,
            0x4020000000000UL, 0x8050000000000UL, 0x110A0000000000UL, 0x22140000000000UL, 0x44280000000000UL, 0x88500000000000UL, 0x10A00000000000UL, 0x20400000000000UL
        ];

        // KING attack masks
        public static readonly ulong[] KING_ATTACK =
        [
            0x302UL, 0x705UL, 0xE0AUL, 0x1C14UL, 0x3828UL, 0x7050UL, 0xE0A0UL, 0xC040UL,
            0x30203UL, 0x70507UL, 0xE0A0EUL, 0x1C141CUL, 0x382838UL, 0x705070UL, 0xE0A0E0UL, 0xC040C0UL,
            0x3020300UL, 0x7050700UL, 0xE0A0E00UL, 0x1C141C00UL, 0x38283800UL, 0x70507000UL, 0xE0A0E000UL, 0xC040C000UL,
            0x302030000UL, 0x705070000UL, 0xE0A0E0000UL, 0x1C141C0000UL, 0x3828380000UL, 0x7050700000UL, 0xE0A0E00000UL, 0xC040C00000UL,
            0x30203000000UL, 0x70507000000UL, 0xE0A0E000000UL, 0x1C141C000000UL, 0x382838000000UL, 0x705070000000UL, 0xE0A0E0000000UL, 0xC040C0000000UL,
            0x3020300000000UL, 0x7050700000000UL, 0xE0A0E00000000UL, 0x1C141C00000000UL, 0x38283800000000UL, 0x70507000000000UL, 0xE0A0E000000000UL, 0xC040C000000000UL,
            0x302030000000000UL, 0x705070000000000UL, 0xE0A0E0000000000UL, 0x1C141C0000000000UL, 0x3828380000000000UL, 0x7050700000000000UL, 0xE0A0E00000000000UL, 0xC040C00000000000UL,
            0x203000000000000UL, 0x507000000000000UL, 0xA0E000000000000UL, 0x141C000000000000UL, 0x2838000000000000UL, 0x5070000000000000UL, 0xA0E0000000000000UL, 0x40C0000000000000UL
        ];

        //Default FEN string
        public static readonly string FEN_DEFAULT = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        //Piece mapping
        public static readonly Dictionary<char, int> PieceMapping = new()
        {
            ['P'] = WHITE_PAWN,
            ['R'] = WHITE_ROOK,
            ['N'] = WHITE_KNIGHT,
            ['B'] = WHITE_BISHOP,
            ['Q'] = WHITE_QUEEN,
            ['K'] = WHITE_KING,
            ['p'] = BLACK_PAWN,
            ['r'] = BLACK_ROOK,
            ['n'] = BLACK_KNIGHT,
            ['b'] = BLACK_BISHOP,
            ['q'] = BLACK_QUEEN,
            ['k'] = BLACK_KING
        };

        /*
         * Mapping for the castling operation. Each element in the array are:
         * Initial rook position
         * New rook position
         * Initial king position
         * New king position
         * Castling turn off bitmask
         */
        public static readonly Dictionary<int, int[]> CastlingMapping = new()
        {
            [WHITE_KING_SIDE] = [0, 2, 3, 1, 3],
            [WHITE_QUEEN_SIDE] = [7, 4, 3, 5, 3],
            [BLACK_KING_SIDE] = [56, 58, 59, 57, 12],
            [BLACK_QUEEN_SIDE] = [63, 60, 59, 61, 12],
        };

        //Material value of each piece type (centipawn)
        public static readonly Dictionary<int, int> Material = new()
        {
            [WHITE_PAWN] = 100,
            [WHITE_ROOK] = 500,
            [WHITE_KNIGHT] = 320,
            [WHITE_BISHOP] = 330,
            [WHITE_QUEEN] = 900,
            [WHITE_KING] = 20000,
        };

        //Piece-square value (https://www.chessprogramming.org/Simplified_Evaluation_Function)
        public static readonly Dictionary<int, int[]> Piece_Square_Table = new()
        {
            [WHITE_PAWN] =
            [
                 0,  0,  0,  0,  0,  0,  0,  0,
                50, 50, 50, 50, 50, 50, 50, 50,
                10, 10, 20, 30, 30, 20, 10, 10,
                 5,  5, 10, 25, 25, 10,  5,  5,
                 0,  0,  0, 20, 20,  0,  0,  0,
                 5, -5,-10,  0,  0,-10, -5,  5,
                 5, 10, 10,-20,-20, 10, 10,  5,
                 0,  0,  0,  0,  0,  0,  0,  0,
            ],

            [WHITE_ROOK] =
            [
                 0,  0,  0,  0,  0,  0,  0,  0,
                 5, 10, 10, 10, 10, 10, 10,  5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                 0,  0,  0,  5,  5,  0,  0,  0,
            ],

            [WHITE_KNIGHT] =
            [
                -50,-40,-30,-30,-30,-30,-40,-50,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -30,  0, 10, 15, 15, 10,  0,-30,
                -30,  5, 15, 20, 20, 15,  5,-30,
                -30,  0, 15, 20, 20, 15,  0,-30,
                -30,  5, 10, 15, 15, 10,  5,-30,
                -40,-20,  0,  5,  5,  0,-20,-40,
                -50,-40,-30,-30,-30,-30,-40,-50,
            ],

            [WHITE_BISHOP] =
            [
                -20,-10,-10,-10,-10,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5, 10, 10,  5,  0,-10,
                -10,  5,  5, 10, 10,  5,  5,-10,
                -10,  0, 10, 10, 10, 10,  0,-10,
                -10, 10, 10, 10, 10, 10, 10,-10,
                -10,  5,  0,  0,  0,  0,  5,-10,
                -20,-10,-10,-10,-10,-10,-10,-20,
            ],

            [WHITE_QUEEN] =
            [
                -20,-10,-10, -5, -5,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5,  5,  5,  5,  0,-10,
                 -5,  0,  5,  5,  5,  5,  0, -5,
                  0,  0,  5,  5,  5,  5,  0, -5,
                -10,  5,  5,  5,  5,  5,  0,-10,
                -10,  0,  5,  0,  0,  0,  0,-10,
                -20,-10,-10, -5, -5,-10,-10,-20,
            ],

            [WHITE_KING] =
            [
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -20,-30,-30,-40,-40,-30,-30,-20,
                -10,-20,-20,-20,-20,-20,-20,-10,
                 20, 20,  0,  0,  0,  0, 20, 20,
                 20, 30, 10,  0,  0, 10, 30, 20,
            ]
        };


    }
}
