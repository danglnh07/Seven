using System.Collections.Concurrent;
using System.Numerics;
using System.Text;

namespace Seven
{
    internal class Chess
    {
        public ulong[] Boards = new ulong[12];
        public int SideToMove;
        public int EnPassantTarget;  // Range: 0-63, or -1
        public int CastlingPrivilege; // 4-bit integer: KQkq
        public int Halfmove;
        public int Fullmove;

        public Chess()
        {
            Clear(); // Initializes to default empty board
        }

        public void Clear()
        {
            for (int i = Constants.WHITE_PAWN; i <= Constants.BLACK_KING; i++)
            {
                Boards[i] = 0;
            }
            EnPassantTarget = -1;
            CastlingPrivilege = 0;
            SideToMove = Constants.WHITE;
            Halfmove = 0;
            Fullmove = 1;
        }

        public void FEN(string fen)
        {
            Clear();

            if (string.IsNullOrEmpty(fen))
            {
                fen = Constants.FEN_DEFAULT;
            }

            string[] data = fen.Split(' ');



            int boardIndex = 63;
            foreach (char piece in data[0])
            {
                if (char.IsDigit(piece))
                {
                    boardIndex -= (piece - '0');
                }
                else if (piece != '/')
                {
                    Boards[Constants.PieceMapping[piece]] |= 1UL << boardIndex;
                    boardIndex--;
                }
            }

            SideToMove = data[1] == "w" ? Constants.WHITE : Constants.BLACK;

            if (data[2] == "-")
            {
                CastlingPrivilege = 0;
            }
            else
            {
                foreach (char c in data[2])
                {
                    if (c == 'K') CastlingPrivilege |= Constants.WHITE_KING_SIDE;
                    else if (c == 'Q') CastlingPrivilege |= Constants.WHITE_QUEEN_SIDE;
                    else if (c == 'k') CastlingPrivilege |= Constants.BLACK_KING_SIDE;
                    else if (c == 'q') CastlingPrivilege |= Constants.BLACK_QUEEN_SIDE;
                }
            }

            EnPassantTarget = data[3] == "-" ? -1 : Util.FromAlgebraicToIndex(data[3]);

            if (data.Length <= 4)
            {
                Halfmove = 0;
                Fullmove = 1;
                return;
            }

            Halfmove = int.TryParse(data[4], out int half) ? half : 0;
            Fullmove = int.TryParse(data[5], out int full) ? full : 1;
        }

        public Chess Clone()
        {
            var clone = new Chess();
            for (int i = Constants.WHITE_PAWN; i <= Constants.BLACK_KING; i++)
            {
                clone.Boards[i] = Boards[i];
            }

            clone.CastlingPrivilege = CastlingPrivilege;
            clone.EnPassantTarget = EnPassantTarget;
            clone.Fullmove = Fullmove;
            clone.Halfmove = Halfmove;
            clone.SideToMove = SideToMove;

            return clone;
        }

        public void Copy(Chess c)
        {
            for (int i = Constants.WHITE_PAWN; i <= Constants.BLACK_KING; i++)
            {
                Boards[i] = c.Boards[i];
            }

            CastlingPrivilege = c.CastlingPrivilege;
            EnPassantTarget = c.EnPassantTarget;
            Fullmove = c.Fullmove;
            Halfmove = c.Halfmove;
            SideToMove = c.SideToMove;
        }

        public void Flip()
        {
            for (int i = 0; i < 6; i++)
            {
                ulong temp = Boards[i];
                Boards[i] = Util.FlipVertical(Boards[i + 6]);
                Boards[i + 6] = Util.FlipVertical(temp);
            }

            SideToMove = SideToMove == Constants.WHITE ? Constants.BLACK : Constants.WHITE;

            if (EnPassantTarget != -1)
                EnPassantTarget = Util.FlipIndexVertical(EnPassantTarget);

            CastlingPrivilege = ((CastlingPrivilege >> 2) | (CastlingPrivilege << 2)) & 15;
        }

        public string[] ToArray()
        {
            string[] board = new string[64];
            string piece;
            string[] pieceMapping = { "P", "R", "N", "B", "Q", "K", "p", "r", "n", "b", "q", "k" };

            for (int index = 0; index < 64; index++)
            {
                piece = " ";
                for (int i = Constants.WHITE_PAWN; i <= Constants.BLACK_KING; i++)
                {
                    if (Util.IsPieceAtIndex(Boards[i], index))
                    {
                        piece = pieceMapping[i];
                        break;
                    }
                }
                board[63 - index] = piece;
            }

            return board;
        }

        public override string ToString()
        {
            var board = ToArray();
            var sb = new StringBuilder();
            sb.AppendLine("+---+---+---+---+---+---+---+---+");

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    sb.AppendFormat("| {0} ", board[8 * i + j]);
                }
                sb.AppendFormat("| {0}\n", 8 - i);
                sb.AppendLine("+---+---+---+---+---+---+---+---+");
            }

            sb.AppendLine("  A   B   C   D   E   F   G   H");

            string enPassantAlgebraic = EnPassantTarget == -1 ? "-" : Util.FromIndexToAlgebraic(EnPassantTarget);

            sb.AppendLine($"Side to move: {SideToMove}");
            sb.AppendLine($"En passant target: {enPassantAlgebraic}");
            sb.AppendLine($"Castling: {CastlingPrivilege}");

            return sb.ToString();
        }

        public ulong GenerateAllWhites()
        {
            var res = 0UL;
            for (int i = Constants.WHITE_PAWN; i <= Constants.WHITE_KING; i++)
            {
                res |= Boards[i];
            }
            return res;
        }

        public ulong GenerateAllBlacks()
        {
            var res = 0UL;
            for (int i = Constants.BLACK_PAWN; i <= Constants.BLACK_KING; i++)
            {
                res |= Boards[i];
            }
            return res;
        }

        public ulong HAndVMoves(int index)
        {
            ulong r = 1UL << index;
            ulong o = GenerateAllWhites() | GenerateAllBlacks();

            ulong m_H = Constants.RANK_MASK[index / 8];
            ulong m_V = Constants.FILE_MASK[7 - index % 8];


            ulong horizontal = (((o & m_H) - 2 * r) ^
                               Util.ReverseBits(Util.ReverseBits(o & m_H) - 2 * Util.ReverseBits(r))) & m_H;

            ulong vertical = (((o & m_V) - 2 * r) ^
                             Util.ReverseBits(Util.ReverseBits(o & m_V) - 2 * Util.ReverseBits(r))) & m_V;

            return horizontal | vertical;
        }

        public ulong DAndAntiDMoves(int index)
        {
            ulong r = 1UL << index;
            ulong o = GenerateAllWhites() | GenerateAllBlacks();

            ulong m_D = Constants.DIAGONAL_MASK[14 - (index / 8 + index % 8)];
            ulong m_AD = Constants.ANTI_DIAGONAL_MASK[7 - (index / 8 - index % 8)];

            ulong diagonal = (((o & m_D) - 2 * r) ^
                             Util.ReverseBits(Util.ReverseBits(o & m_D) - 2 * Util.ReverseBits(r))) & m_D;

            ulong antiDiagonal = (((o & m_AD) - 2 * r) ^
                                 Util.ReverseBits(Util.ReverseBits(o & m_AD) - 2 * Util.ReverseBits(r))) & m_AD;

            return diagonal | antiDiagonal;
        }

        public ulong GenerateWhiteKingInDanger()
        {
            ulong blacksEmptyOrWhiteKing = ~GenerateAllWhites() | Boards[Constants.WHITE_KING];
            ulong FILE_A = Constants.FILE_MASK[0];
            ulong FILE_H = Constants.FILE_MASK[7];
            ulong temp, whiteInDanger = 0, wk;
            int index;

            // Temporarily remove the White King
            wk = Boards[Constants.WHITE_KING];
            Boards[Constants.WHITE_KING] = 0;

            // Black pawn attacks (ignores en passant)
            whiteInDanger |= (Boards[Constants.BLACK_PAWN] >> 9) & blacksEmptyOrWhiteKing & ~FILE_A;
            whiteInDanger |= (Boards[Constants.BLACK_PAWN] >> 7) & blacksEmptyOrWhiteKing & ~FILE_H;

            // Black rooks and queens (horizontal/vertical)
            temp = Boards[Constants.BLACK_ROOK] | Boards[Constants.BLACK_QUEEN];
            while (temp != 0)
            {
                index = BitOperations.TrailingZeroCount(temp);
                whiteInDanger |= HAndVMoves(index) & blacksEmptyOrWhiteKing;
                Util.ClearBit(index, ref temp);
            }

            // Black knights
            temp = Boards[Constants.BLACK_KNIGHT];
            while (temp != 0)
            {
                index = BitOperations.TrailingZeroCount(temp);
                whiteInDanger |= Constants.KNIGHT_ATTACK[index] & blacksEmptyOrWhiteKing;
                Util.ClearBit(index, ref temp);
            }

            // Black bishops and queens (diagonals)
            temp = Boards[Constants.BLACK_BISHOP] | Boards[Constants.BLACK_QUEEN];
            while (temp != 0)
            {
                index = BitOperations.TrailingZeroCount(temp);
                whiteInDanger |= DAndAntiDMoves(index) & blacksEmptyOrWhiteKing;
                Util.ClearBit(index, ref temp);
            }

            // Black king (only one, no loop)
            index = BitOperations.TrailingZeroCount(Boards[Constants.BLACK_KING]);
            whiteInDanger |= Constants.KING_ATTACK[index] & blacksEmptyOrWhiteKing;

            // Restore White King
            Boards[Constants.WHITE_KING] = wk;

            return whiteInDanger;
        }

        public ulong GenerateWhiteAttacks()
        {
            ulong FILE_A = Constants.FILE_MASK[0];
            ulong FILE_H = Constants.FILE_MASK[7];

            ulong whitesCanLandTo = ~GenerateAllWhites();
            ulong blacks = GenerateAllBlacks();
            ulong temp, whiteCanAttack = 0;
            int index;

            // White pawn attacks
            whiteCanAttack |= (Boards[Constants.WHITE_PAWN] << 9) & blacks & ~FILE_H;
            whiteCanAttack |= (Boards[Constants.WHITE_PAWN] << 7) & blacks & ~FILE_A;

            if (EnPassantTarget >= 40 && EnPassantTarget <= 47)
            {
                whiteCanAttack |= 1UL << EnPassantTarget;
            }

            // White rook and queen (horizontal/vertical)
            temp = Boards[Constants.WHITE_ROOK] | Boards[Constants.WHITE_QUEEN];
            while (temp != 0)
            {
                index = BitOperations.TrailingZeroCount(temp);
                whiteCanAttack |= HAndVMoves(index) & whitesCanLandTo;
                Util.ClearBit(index, ref temp);
            }

            // White knights
            temp = Boards[Constants.WHITE_KNIGHT];
            while (temp != 0)
            {
                index = BitOperations.TrailingZeroCount(temp);
                whiteCanAttack |= Constants.KNIGHT_ATTACK[index] & whitesCanLandTo;
                Util.ClearBit(index, ref temp);
            }

            // White bishop and queen (diagonal/anti-diagonal)
            temp = Boards[Constants.WHITE_BISHOP] | Boards[Constants.WHITE_QUEEN];
            while (temp != 0)
            {
                index = BitOperations.TrailingZeroCount(temp);
                whiteCanAttack |= DAndAntiDMoves(index) & whitesCanLandTo;
                Util.ClearBit(index, ref temp);
            }

            // White king (only one)
            index = BitOperations.TrailingZeroCount(Boards[Constants.WHITE_KING]);
            whiteCanAttack |= Constants.KING_ATTACK[index] & whitesCanLandTo;

            return whiteCanAttack;
        }

        public bool IsWhiteKingChecked()
        {
            Flip();
            var blackAttacks = Util.FlipVertical(GenerateWhiteAttacks());
            Flip();
            return (Boards[Constants.WHITE_KING] & blackAttacks) != 0;
        }

        public bool IsBlackKingChecked()
        {
            return (Boards[Constants.BLACK_KING] & GenerateWhiteAttacks()) != 0;
        }

        public (ulong attackers, bool hasSlidingPieceAttacker) CalculateWhiteKingAttackers()
        {
            ulong FILE_A = Constants.FILE_MASK[0];
            ulong FILE_H = Constants.FILE_MASK[7];
            int kingIndex = BitOperations.TrailingZeroCount(Boards[Constants.WHITE_KING]);

            // Pawn attacks toward the white king
            ulong pawnAttackers = (Boards[Constants.WHITE_KING] << 7) & ~FILE_A & Boards[Constants.BLACK_PAWN];
            pawnAttackers |= (Boards[Constants.WHITE_KING] << 9) & ~FILE_H & Boards[Constants.BLACK_PAWN];

            // Rook attacks
            ulong rookAttackers = HAndVMoves(kingIndex) & Boards[Constants.BLACK_ROOK];

            // Knight attacks
            ulong knightAttackers = Constants.KNIGHT_ATTACK[kingIndex] & Boards[Constants.BLACK_KNIGHT];

            // Bishop attacks
            ulong bishopAttackers = DAndAntiDMoves(kingIndex) & Boards[Constants.BLACK_BISHOP];

            // Queen attacks
            ulong queenAttackers = (HAndVMoves(kingIndex) | DAndAntiDMoves(kingIndex)) & Boards[Constants.BLACK_QUEEN];

            ulong allAttackers = pawnAttackers | rookAttackers | knightAttackers | bishopAttackers | queenAttackers;

            bool hasSliding = rookAttackers != 0 || bishopAttackers != 0 || queenAttackers != 0;

            return (allAttackers, hasSliding);
        }

        public List<Move> WhiteKingMoves()
        {
            int kingIndex = BitOperations.TrailingZeroCount(Boards[Constants.WHITE_KING]);
            ulong kingMoves = Constants.KING_ATTACK[kingIndex] & ~GenerateWhiteKingInDanger();
            ulong blacks = GenerateAllBlacks();
            ulong empty = ~(GenerateAllWhites() | blacks);

            var moves = new List<Move>();

            kingMoves &= (empty | blacks);
            while (kingMoves != 0)
            {
                int index = BitOperations.TrailingZeroCount(kingMoves);
                moves.Add(new Move(Constants.WHITE_KING, Constants.WHITE_KING, kingIndex, index));
                Util.ClearBit(index, ref kingMoves);
            }

            return moves;
        }

        public List<Move> EnPassantMoves()
        {
            ulong FILE_A = Constants.FILE_MASK[0];
            ulong FILE_H = Constants.FILE_MASK[7];

            var moves = new List<Move>();

            // Only allow en passant targets on rank 5
            if (EnPassantTarget >= 40 && EnPassantTarget <= 47)
            {
                ulong wpTemp = Boards[Constants.WHITE_PAWN];
                ulong bpTemp = Boards[Constants.BLACK_PAWN];

                ulong epMove = ((1UL << EnPassantTarget) >> 9) & Boards[Constants.WHITE_PAWN] & ~FILE_A;
                epMove |= ((1UL << EnPassantTarget) >> 7) & Boards[Constants.WHITE_PAWN] & ~FILE_H;

                while (epMove != 0)
                {
                    int index = BitOperations.TrailingZeroCount(epMove);

                    // Simulate en passant move
                    Util.ClearBit(index, ref Boards[Constants.WHITE_PAWN]);
                    Util.SetBit(EnPassantTarget, ref Boards[Constants.WHITE_PAWN]);
                    Util.ClearBit(EnPassantTarget - 8, ref Boards[Constants.BLACK_PAWN]);

                    if (!IsWhiteKingChecked())
                    {
                        moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, index, EnPassantTarget));
                    }

                    // Restore state
                    Boards[Constants.WHITE_PAWN] = wpTemp;
                    Boards[Constants.BLACK_PAWN] = bpTemp;

                    Util.ClearBit(index, ref epMove);
                }
            }

            return moves;
        }

        public static List<Move> PinSPMoves(int movePiece, int capturePiece, int pseudoAttackerIndex, int pinPieceIndex, ulong rayline)
        {
            List<Move> moves = [];

            // Capture move
            moves.Add(new Move(movePiece, movePiece, pinPieceIndex, pseudoAttackerIndex));

            // Remove the pin piece from the rayline
            Util.ClearBit(pinPieceIndex, ref rayline);

            // Add any remaining legal (non-capture) moves along the pin line
            while (rayline != 0)
            {
                int index = BitOperations.TrailingZeroCount(rayline);
                moves.Add(new Move(movePiece, movePiece, pinPieceIndex, index));
                Util.ClearBit(index, ref rayline);
            }

            return moves;
        }

        public static List<Move> PinPawnMovesInFile(int pinPieceIndex, ulong empty)
        {
            var moves = new List<Move>();

            ulong pawnMoves = (1UL << (pinPieceIndex + 8)) & empty;
            pawnMoves |= (1UL << (pinPieceIndex + 16)) & empty & (empty << 8) & Constants.RANK_MASK[3];

            while (pawnMoves != 0)
            {
                int index = BitOperations.TrailingZeroCount(pawnMoves);
                moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, pinPieceIndex, index));
                Util.ClearBit(index, ref pawnMoves);
            }

            return moves;
        }

        public static List<Move> PinPawnMovesInDiagonals(int direction, int pinPieceIndex, int pseudoAttackerIndex, int capturePiece)
        {
            var moves = new List<Move>();

            ulong pawnMoves = (1UL << (pinPieceIndex + direction)) & (1UL << pseudoAttackerIndex);

            if (pawnMoves != 0)
            {
                int index = BitOperations.TrailingZeroCount(pawnMoves);

                if (pseudoAttackerIndex >= 56 && pseudoAttackerIndex <= 63)
                {
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_QUEEN, pinPieceIndex, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_ROOK, pinPieceIndex, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_BISHOP, pinPieceIndex, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_KNIGHT, pinPieceIndex, index));
                }
                else
                {
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, pinPieceIndex, index));
                }
            }

            return moves;
        }

        public List<Move> CaptureAttackerMoves(ulong attacker, ulong wp, ulong wr, ulong wn, ulong wb, ulong wq, int attackerIndex)
        {
            var moves = new List<Move>();
            ulong FILE_A = Constants.FILE_MASK[0];
            ulong FILE_H = Constants.FILE_MASK[7];
            int index;

            // Pawn captures
            ulong capture = (attacker >> 7) & ~FILE_H & wp;
            capture |= (attacker >> 9) & ~FILE_A & wp;

            if (attackerIndex >= 56 && attackerIndex <= 63) // Promotion
            {
                while (capture != 0)
                {
                    index = BitOperations.TrailingZeroCount(capture);
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_QUEEN, index, attackerIndex));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_ROOK, index, attackerIndex));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_BISHOP, index, attackerIndex));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_KNIGHT, index, attackerIndex));
                    Util.ClearBit(index, ref capture);
                }
            }
            else
            {
                while (capture != 0)
                {
                    index = BitOperations.TrailingZeroCount(capture);
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, index, attackerIndex));
                    Util.ClearBit(index, ref capture);
                }
            }

            // Rook captures
            capture = HAndVMoves(attackerIndex) & wr;
            while (capture != 0)
            {
                index = BitOperations.TrailingZeroCount(capture);
                moves.Add(new Move(Constants.WHITE_ROOK, Constants.WHITE_ROOK, index, attackerIndex));
                Util.ClearBit(index, ref capture);
            }

            // Knight captures
            capture = Constants.KNIGHT_ATTACK[attackerIndex] & wn;
            while (capture != 0)
            {
                index = BitOperations.TrailingZeroCount(capture);
                moves.Add(new Move(Constants.WHITE_KNIGHT, Constants.WHITE_KNIGHT, index, attackerIndex));
                Util.ClearBit(index, ref capture);
            }

            // Bishop captures
            capture = DAndAntiDMoves(attackerIndex) & wb;
            while (capture != 0)
            {
                index = BitOperations.TrailingZeroCount(capture);
                moves.Add(new Move(Constants.WHITE_BISHOP, Constants.WHITE_BISHOP, index, attackerIndex));
                Util.ClearBit(index, ref capture);
            }

            // Queen captures
            capture = (HAndVMoves(attackerIndex) | DAndAntiDMoves(attackerIndex)) & wq;
            while (capture != 0)
            {
                index = BitOperations.TrailingZeroCount(capture);
                moves.Add(new Move(Constants.WHITE_QUEEN, Constants.WHITE_QUEEN, index, attackerIndex));
                Util.ClearBit(index, ref capture);
            }

            return moves;
        }

        public List<Move> BlockingAttackerMoves(ulong wp, ulong wr, ulong wn, ulong wb, ulong wq, int attackerIndex, int kingIndex)
        {
            var moves = new List<Move>();
            ulong blockMoves = 0;
            int direction = 0;
            int index;
            ulong empty = ~(GenerateAllWhites() | GenerateAllBlacks());
            ulong RANK_2 = Constants.RANK_MASK[1];

            int min = Util.Min(attackerIndex, kingIndex);
            int max = Util.Max(attackerIndex, kingIndex);

            if (Util.IsAtSameRank(attackerIndex, kingIndex))
            {
                direction = Constants.RANK;

                for (int i = min + direction; i < max; i += direction)
                {
                    blockMoves = ((1UL << i) >> 8) & wp;
                    blockMoves |= ((1UL << i) >> 16) & (empty >> 8) & wp & RANK_2;

                    if (i >= 56 && i <= 63)
                    {
                        while (blockMoves != 0)
                        {
                            index = BitOperations.TrailingZeroCount(blockMoves);
                            moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_QUEEN, index, i));
                            moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_ROOK, index, i));
                            moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_BISHOP, index, i));
                            moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_KNIGHT, index, i));
                            Util.ClearBit(index, ref blockMoves);
                        }
                    }
                    else
                    {
                        while (blockMoves != 0)
                        {
                            index = BitOperations.TrailingZeroCount(blockMoves);
                            moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, index, i));
                            Util.ClearBit(index, ref blockMoves);
                        }
                    }
                }
            }
            else if (Util.IsAtSameFile(attackerIndex, kingIndex))
            {
                direction = Constants.FILE;
            }
            else if (Util.IsAtSameDiagonal(attackerIndex, kingIndex))
            {
                direction = Constants.DIAGONAL;
            }
            else if (Util.IsAtSameAntiDiagonal(attackerIndex, kingIndex))
            {
                direction = Constants.ANTI_DIAGONAL;
            }

            for (int i = min + direction; i < max; i += direction)
            {
                blockMoves = 0;

                // Pinned pawn blocking (diagonal / anti-diagonal only)
                if (direction == Constants.DIAGONAL || direction == Constants.ANTI_DIAGONAL)
                {
                    blockMoves |= ((1UL << i) >> 8) & wp;
                    blockMoves |= ((1UL << i) >> 16) & (empty >> 8) & wp & RANK_2;

                    while (blockMoves != 0)
                    {
                        index = BitOperations.TrailingZeroCount(blockMoves);
                        moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, index, i));
                        Util.ClearBit(index, ref blockMoves);
                    }
                }

                // Rook
                blockMoves = HAndVMoves(i) & wr;
                while (blockMoves != 0)
                {
                    index = BitOperations.TrailingZeroCount(blockMoves);
                    moves.Add(new Move(Constants.WHITE_ROOK, Constants.WHITE_ROOK, index, i));
                    Util.ClearBit(index, ref blockMoves);
                }

                // Knight
                blockMoves = Constants.KNIGHT_ATTACK[i] & wn;
                while (blockMoves != 0)
                {
                    index = BitOperations.TrailingZeroCount(blockMoves);
                    moves.Add(new Move(Constants.WHITE_KNIGHT, Constants.WHITE_KNIGHT, index, i));
                    Util.ClearBit(index, ref blockMoves);
                }

                // Bishop
                blockMoves = DAndAntiDMoves(i) & wb;
                while (blockMoves != 0)
                {
                    index = BitOperations.TrailingZeroCount(blockMoves);
                    moves.Add(new Move(Constants.WHITE_BISHOP, Constants.WHITE_BISHOP, index, i));
                    Util.ClearBit(index, ref blockMoves);
                }

                // Queen
                blockMoves = (HAndVMoves(i) | DAndAntiDMoves(i)) & wq;
                while (blockMoves != 0)
                {
                    index = BitOperations.TrailingZeroCount(blockMoves);
                    moves.Add(new Move(Constants.WHITE_QUEEN, Constants.WHITE_QUEEN, index, i));
                    Util.ClearBit(index, ref blockMoves);
                }
            }

            return moves;
        }

        public List<Move> PseudoLegalMoves(ulong wp, ulong wr, ulong wn, ulong wb, ulong wq)
        {
            var moves = new List<Move>();
            int pieceIndex, index;

            ulong pawnMoves, rookMoves, knightMoves, bishopMoves, queenMoves;

            ulong whites = GenerateAllWhites();
            ulong blacks = GenerateAllBlacks();
            ulong empty = ~(whites | blacks);
            ulong RANK_4 = Constants.RANK_MASK[3];
            ulong FILE_A = Constants.FILE_MASK[0];
            ulong FILE_H = Constants.FILE_MASK[7];

            // === Pawn Single Push ===
            pawnMoves = (wp << 8) & empty;
            while (pawnMoves != 0)
            {
                index = BitOperations.TrailingZeroCount(pawnMoves);

                if (index >= 56 && index <= 63)
                {
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_QUEEN, index - 8, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_ROOK, index - 8, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_BISHOP, index - 8, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_KNIGHT, index - 8, index));
                }
                else
                {
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, index - 8, index));
                }

                Util.ClearBit(index, ref pawnMoves);
            }

            // === Pawn Double Push ===
            pawnMoves = (wp << 16) & empty & (empty << 8) & RANK_4;
            while (pawnMoves != 0)
            {
                index = BitOperations.TrailingZeroCount(pawnMoves);
                moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, index - 16, index));
                Util.ClearBit(index, ref pawnMoves);
            }

            // === Pawn Captures (Right) ===
            pawnMoves = (wp << 7) & blacks & ~FILE_A;
            while (pawnMoves != 0)
            {
                index = BitOperations.TrailingZeroCount(pawnMoves);

                if (index >= 56 && index <= 63)
                {
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_QUEEN, index - 7, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_ROOK, index - 7, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_BISHOP, index - 7, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_KNIGHT, index - 7, index));
                }
                else
                {
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, index - 7, index));
                }

                Util.ClearBit(index, ref pawnMoves);
            }

            // === Pawn Captures (Left) ===
            pawnMoves = (wp << 9) & blacks & ~FILE_H;
            while (pawnMoves != 0)
            {
                index = BitOperations.TrailingZeroCount(pawnMoves);

                if (index >= 56 && index <= 63)
                {
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_QUEEN, index - 9, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_ROOK, index - 9, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_BISHOP, index - 9, index));
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_KNIGHT, index - 9, index));
                }
                else
                {
                    moves.Add(new Move(Constants.WHITE_PAWN, Constants.WHITE_PAWN, index - 9, index));
                }

                Util.ClearBit(index, ref pawnMoves);
            }

            // === Rooks ===
            while (wr != 0)
            {
                pieceIndex = BitOperations.TrailingZeroCount(wr);
                rookMoves = HAndVMoves(pieceIndex) & ~whites;
                while (rookMoves != 0)
                {
                    index = BitOperations.TrailingZeroCount(rookMoves);
                    moves.Add(new Move(Constants.WHITE_ROOK, Constants.WHITE_ROOK, pieceIndex, index));
                    Util.ClearBit(index, ref rookMoves);
                }

                Util.ClearBit(pieceIndex, ref wr);
            }

            // === Knights ===
            while (wn != 0)
            {
                pieceIndex = BitOperations.TrailingZeroCount(wn);
                knightMoves = Constants.KNIGHT_ATTACK[pieceIndex] & ~whites;
                while (knightMoves != 0)
                {
                    index = BitOperations.TrailingZeroCount(knightMoves);
                    moves.Add(new Move(Constants.WHITE_KNIGHT, Constants.WHITE_KNIGHT, pieceIndex, index));
                    Util.ClearBit(index, ref knightMoves);
                }

                Util.ClearBit(pieceIndex, ref wn);
            }

            // === Bishops ===
            while (wb != 0)
            {
                pieceIndex = BitOperations.TrailingZeroCount(wb);
                bishopMoves = DAndAntiDMoves(pieceIndex) & ~whites;
                while (bishopMoves != 0)
                {
                    index = BitOperations.TrailingZeroCount(bishopMoves);
                    moves.Add(new Move(Constants.WHITE_BISHOP, Constants.WHITE_BISHOP, pieceIndex, index));
                    Util.ClearBit(index, ref bishopMoves);
                }

                Util.ClearBit(pieceIndex, ref wb);
            }

            // === Queens ===
            while (wq != 0)
            {
                pieceIndex = BitOperations.TrailingZeroCount(wq);
                queenMoves = (HAndVMoves(pieceIndex) | DAndAntiDMoves(pieceIndex)) & ~whites;
                while (queenMoves != 0)
                {
                    index = BitOperations.TrailingZeroCount(queenMoves);
                    moves.Add(new Move(Constants.WHITE_QUEEN, Constants.WHITE_QUEEN, pieceIndex, index));
                    Util.ClearBit(index, ref queenMoves);
                }

                Util.ClearBit(pieceIndex, ref wq);
            }

            return moves;
        }

        public List<Move> WhiteMoveGeneration()
        {
            var moves = new List<Move>();

            // Temporary bitboards
            ulong wp = Boards[Constants.WHITE_PAWN];
            ulong wr = Boards[Constants.WHITE_ROOK];
            ulong wn = Boards[Constants.WHITE_KNIGHT];
            ulong wb = Boards[Constants.WHITE_BISHOP];
            ulong wq = Boards[Constants.WHITE_QUEEN];

            int kingIndex = BitOperations.TrailingZeroCount(Boards[Constants.WHITE_KING]);
            ulong whites = GenerateAllWhites();
            ulong blacks = GenerateAllBlacks();
            ulong empty = ~(whites | blacks);

            // 1. King moves
            moves.AddRange(WhiteKingMoves());

            // 2. Calculate attackers
            (ulong attackers, bool hasSPAttacker) = CalculateWhiteKingAttackers();

            // Double check or king alone => only king moves
            if (BitOperations.PopCount(attackers) > 1 || (wp | wr | wn | wb | wq) == 0)
                return moves;

            // 3. En Passant
            moves.AddRange(EnPassantMoves());

            // 4. Calculate pinned piece moves
            var pinPieceMoves = new List<Move>();
            ulong temp, rayline;
            int min, max, pseudoAttackerIndex, pinPieceIndex, direction, capturePiece;

            // Pins from rooks & queens
            temp = Boards[Constants.BLACK_ROOK] | Boards[Constants.BLACK_QUEEN];
            while (temp != 0)
            {
                pseudoAttackerIndex = BitOperations.TrailingZeroCount(temp);
                capturePiece = Util.IsPieceAtIndex(Boards[Constants.BLACK_ROOK], pseudoAttackerIndex)
                    ? Constants.BLACK_ROOK : Constants.BLACK_QUEEN;

                min = Util.Min(kingIndex, pseudoAttackerIndex);
                max = Util.Max(kingIndex, pseudoAttackerIndex);

                if (Util.IsAtSameRank(kingIndex, pseudoAttackerIndex))
                    direction = Constants.RANK;
                else if (Util.IsAtSameFile(kingIndex, pseudoAttackerIndex))
                    direction = Constants.FILE;
                else
                    direction = -1;

                if (direction != -1)
                {
                    rayline = Util.CalculateRayAttackLine(min, max, direction);

                    if (BitOperations.PopCount(rayline & blacks) == 0 &&
                        BitOperations.PopCount(rayline & whites) == 1)
                    {
                        pinPieceIndex = BitOperations.TrailingZeroCount(rayline & whites);

                        if (Util.IsPieceAtIndex(wr, pinPieceIndex))
                        {
                            pinPieceMoves.AddRange(PinSPMoves(Constants.WHITE_ROOK, capturePiece, pseudoAttackerIndex, pinPieceIndex, rayline));
                        }
                        else if (Util.IsPieceAtIndex(wq, pinPieceIndex))
                        {
                            pinPieceMoves.AddRange(PinSPMoves(Constants.WHITE_QUEEN, capturePiece, pseudoAttackerIndex, pinPieceIndex, rayline));
                        }
                        else if (direction == Constants.FILE && Util.IsPieceAtIndex(wp, pinPieceIndex))
                        {
                            pinPieceMoves.AddRange(PinPawnMovesInFile(pinPieceIndex, empty));
                        }
                        Util.ClearBit(pinPieceIndex, ref wp);
                        Util.ClearBit(pinPieceIndex, ref wr);
                        Util.ClearBit(pinPieceIndex, ref wn);
                        Util.ClearBit(pinPieceIndex, ref wb);
                        Util.ClearBit(pinPieceIndex, ref wq);
                    }
                }

                Util.ClearBit(pseudoAttackerIndex, ref temp);
            }

            // Pins from bishops & queens
            temp = Boards[Constants.BLACK_BISHOP] | Boards[Constants.BLACK_QUEEN];
            while (temp != 0)
            {
                pseudoAttackerIndex = BitOperations.TrailingZeroCount(temp);
                capturePiece = Util.IsPieceAtIndex(Boards[Constants.BLACK_BISHOP], pseudoAttackerIndex)
                    ? Constants.BLACK_BISHOP : Constants.BLACK_QUEEN;

                min = Util.Min(kingIndex, pseudoAttackerIndex);
                max = Util.Max(kingIndex, pseudoAttackerIndex);

                if (Util.IsAtSameDiagonal(kingIndex, pseudoAttackerIndex))
                    direction = Constants.DIAGONAL;
                else if (Util.IsAtSameAntiDiagonal(kingIndex, pseudoAttackerIndex))
                    direction = Constants.ANTI_DIAGONAL;
                else
                    direction = -1;

                if (direction != -1)
                {
                    rayline = Util.CalculateRayAttackLine(min, max, direction);

                    if (BitOperations.PopCount(rayline & blacks) == 0 &&
                        BitOperations.PopCount(rayline & whites) == 1)
                    {
                        pinPieceIndex = BitOperations.TrailingZeroCount(rayline & whites);

                        if (Util.IsPieceAtIndex(wb, pinPieceIndex))
                        {
                            pinPieceMoves.AddRange(PinSPMoves(Constants.WHITE_BISHOP, capturePiece, pseudoAttackerIndex, pinPieceIndex, rayline));
                        }
                        else if (Util.IsPieceAtIndex(wq, pinPieceIndex))
                        {
                            pinPieceMoves.AddRange(PinSPMoves(Constants.WHITE_QUEEN, capturePiece, pseudoAttackerIndex, pinPieceIndex, rayline));
                        }
                        else if (Util.IsPieceAtIndex(wp, pinPieceIndex))
                        {
                            pinPieceMoves.AddRange(PinPawnMovesInDiagonals(direction, pinPieceIndex, pseudoAttackerIndex, Constants.BLACK_BISHOP));
                        }

                        Util.ClearBit(pinPieceIndex, ref wp);
                        Util.ClearBit(pinPieceIndex, ref wr);
                        Util.ClearBit(pinPieceIndex, ref wn);
                        Util.ClearBit(pinPieceIndex, ref wb);
                        Util.ClearBit(pinPieceIndex, ref wq);
                    }
                }

                Util.ClearBit(pseudoAttackerIndex, ref temp);
            }

            // 5. Single check response: capture or block
            if (BitOperations.PopCount(attackers) == 1)
            {
                int attackerIndex = BitOperations.TrailingZeroCount(attackers);

                moves.AddRange(CaptureAttackerMoves(attackers, wp, wr, wn, wb, wq, attackerIndex));

                if (hasSPAttacker)
                {
                    moves.AddRange(BlockingAttackerMoves(wp, wr, wn, wb, wq, attackerIndex, kingIndex));
                }

                return moves;
            }

            // 6. No check: generate pseudo-legal and pinned piece moves
            moves.AddRange(PseudoLegalMoves(wp, wr, wn, wb, wq));
            moves.AddRange(pinPieceMoves);

            // 7. Castling
            ulong whiteKingInDanger = GenerateWhiteKingInDanger();

            if ((CastlingPrivilege & Constants.WHITE_KING_SIDE) == Constants.WHITE_KING_SIDE &&
                (empty & 0x6) == 0x6 && (whiteKingInDanger & 0xE) == 0)
            {
                //move.Castling = Constants.WHITE_KING_SIDE;
                moves.Add(new Move(Constants.WHITE_KING_SIDE));
            }

            if ((CastlingPrivilege & Constants.WHITE_QUEEN_SIDE) == Constants.WHITE_QUEEN_SIDE &&
                (empty & 0x70) == 0x70 && (whiteKingInDanger & 0x38) == 0)
            {
                //move.Castling = Constants.WHITE_QUEEN_SIDE;
                moves.Add(new Move(Constants.WHITE_QUEEN_SIDE));
            }

            return moves;
        }

        public List<Move> MoveGeneration()
        {
            if (SideToMove == Constants.BLACK)
            {
                Flip(); // Flip board to use white move logic
                var moves = WhiteMoveGeneration();
                Flip(); // Flip back to restore original state

                var reflectedMoves = new List<Move>();
                foreach (var move in moves)
                {
                    switch (move.Castling)
                    {
                        case Constants.WHITE_KING_SIDE:
                            reflectedMoves.Add(new Move(Constants.BLACK_KING_SIDE));
                            break;
                        case Constants.WHITE_QUEEN_SIDE:
                            reflectedMoves.Add(new Move(Constants.BLACK_QUEEN_SIDE));
                            break;
                        default:
                            reflectedMoves.Add(new Move(move.FromBoard + 6, move.ToBoard + 6,
                                Util.FlipIndexVertical(move.FromIndex), Util.FlipIndexVertical(move.ToIndex)));
                            break;
                    }
                }

                return reflectedMoves;
            }

            return WhiteMoveGeneration();
        }

        private void Castling(int cs)
        {
            EnPassantTarget = -1;
            Halfmove++;

            if (cs == Constants.WHITE_KING_SIDE || cs == Constants.WHITE_QUEEN_SIDE)
            {
                Util.ClearBit(Constants.CastlingMapping[cs][0], ref Boards[Constants.WHITE_ROOK]);
                Util.SetBit(Constants.CastlingMapping[cs][1], ref Boards[Constants.WHITE_ROOK]);
                Util.ClearBit(Constants.CastlingMapping[cs][2], ref Boards[Constants.WHITE_KING]);
                Util.SetBit(Constants.CastlingMapping[cs][3], ref Boards[Constants.WHITE_KING]);
            }
            else
            {
                Util.ClearBit(Constants.CastlingMapping[cs][0], ref Boards[Constants.BLACK_ROOK]);
                Util.SetBit(Constants.CastlingMapping[cs][1], ref Boards[Constants.BLACK_ROOK]);
                Util.ClearBit(Constants.CastlingMapping[cs][2], ref Boards[Constants.BLACK_KING]);
                Util.SetBit(Constants.CastlingMapping[cs][3], ref Boards[Constants.BLACK_KING]);

                // Fullmove only increments on Black's turn
                Fullmove++;
            }

            SideToMove = Constants.WHITE + Constants.BLACK - SideToMove;
            CastlingPrivilege &= Constants.CastlingMapping[cs][4];
        }

        public void MakeMove(Move move)
        {
            if (move.Castling != 0)
            {
                Castling(move.Castling);
                return;
            }

            // 1. Move the piece
            Util.ClearBit(move.FromIndex, ref Boards[move.FromBoard]);
            Util.SetBit(move.ToIndex, ref Boards[move.ToBoard]);

            // 2. Handle captures
            int captureIndex = move.ToIndex;
            if (SideToMove == Constants.WHITE)
            {
                if (move.FromBoard == Constants.WHITE_PAWN && move.ToIndex == EnPassantTarget)
                {
                    captureIndex = EnPassantTarget - 8;
                    Util.ClearBit(captureIndex, ref Boards[Constants.BLACK_PAWN]);
                }
                else
                {
                    for (int i = Constants.BLACK_PAWN; i < Constants.BLACK_KING; i++)
                        Util.ClearBit(captureIndex, ref Boards[i]);
                }
            }
            else // Black to move
            {
                if (move.FromBoard == Constants.BLACK_PAWN && move.ToIndex == EnPassantTarget)
                {
                    captureIndex = EnPassantTarget + 8;
                    Util.ClearBit(captureIndex, ref Boards[Constants.WHITE_PAWN]);
                }
                else
                {
                    for (int i = Constants.WHITE_PAWN; i < Constants.WHITE_KING; i++)
                        Util.ClearBit(captureIndex, ref Boards[i]);
                }
            }

            // 3. Handle en passant target square (only after double push)
            if ((move.FromBoard == Constants.WHITE_PAWN || move.FromBoard == Constants.BLACK_PAWN) &&
                Math.Abs(move.ToIndex - move.FromIndex) == 16)
            {
                EnPassantTarget = (move.ToIndex + move.FromIndex) / 2;
            }
            else
            {
                EnPassantTarget = -1;
            }

            // 4. Update castling rights
            if (!Util.IsPieceAtIndex(Boards[Constants.WHITE_KING], 3))
            {
                CastlingPrivilege &= 0b0011;
            }
            else
            {
                if (!Util.IsPieceAtIndex(Boards[Constants.WHITE_ROOK], 0))
                    CastlingPrivilege &= 0b0111;

                if (!Util.IsPieceAtIndex(Boards[Constants.WHITE_ROOK], 7))
                    CastlingPrivilege &= 0b1011;
            }

            if (!Util.IsPieceAtIndex(Boards[Constants.BLACK_KING], 59))
            {
                CastlingPrivilege &= 0b1100;
            }
            else
            {
                if (!Util.IsPieceAtIndex(Boards[Constants.BLACK_ROOK], 56))
                    CastlingPrivilege &= 0b1101;

                if (!Util.IsPieceAtIndex(Boards[Constants.BLACK_ROOK], 63))
                    CastlingPrivilege &= 0b1110;
            }

            // 5. Change side to move
            SideToMove = Constants.WHITE + Constants.BLACK - SideToMove;

            // 6. Halfmove clock (reset on pawn move or capture)
            if (move.FromBoard == Constants.WHITE_PAWN ||
                move.FromBoard == Constants.BLACK_PAWN ||
                Util.IsPieceAtIndex(GenerateAllBlacks(), captureIndex))
            {
                Halfmove = 0;
            }
            else
            {
                Halfmove++;
            }

            // 7. Fullmove count increments only after Black's move
            if (SideToMove == Constants.BLACK)
            {
                Fullmove++;
            }
        }

        public int Perft(int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            var count = 0;
            var moves = MoveGeneration();
            foreach (var move in moves)
            {
                var clone = Clone();
                clone.MakeMove(move);
                count += clone.Perft(depth - 1);
            }
            return count;
        }

        public Dictionary<string, int> DividePerft(int depth)
        {
            if (depth == 0)
            {
                return [];
            }

            Dictionary<string, int> result = new();
            var moves = MoveGeneration();
            var count = 0;
            foreach (var move in moves)
            {
                var clone = Clone();
                clone.MakeMove(move);
                count = clone.Perft(depth - 1);
                result.Add(move.ToString(), count);
            }
            return result;
        }

        public Dictionary<string, int> FastPerft(int depth)
        {
            // If depth is small, just use DividePerft to avoid threading overhead
            if (depth <= 3)
            {
                return DividePerft(depth) ?? [];
            }

            var result = new ConcurrentDictionary<string, int>();
            var moves = MoveGeneration();
            var tasks = new List<Task>();

            foreach (var move in moves)
            {
                tasks.Add(Task.Run(() =>
                {
                    var clone = Clone();
                    clone.MakeMove(move);
                    int count = clone.Perft(depth - 1);
                    result[move.ToString()] = count;
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static int EvaluatePS(ulong bitboard, int[] pieceSquare, int side)
        {
            int score = 0;

            while (bitboard != 0)
            {
                int trailing = BitOperations.TrailingZeroCount(bitboard);
                int index = 63 - trailing; // reverse index for bitboard layout

                if (side == Constants.BLACK)
                    score += pieceSquare[Util.FlipIndexVertical(index)];
                else
                    score += pieceSquare[index];

                Util.ClearBit(trailing, ref bitboard);
            }

            return score;
        }

        public int CalculateBonus()
        {
            int bonus = 0;

            if (SideToMove == Constants.BLACK)
            {
                Flip();
                bonus = CalculateBonus(); // recursive after flip
                Flip();
                return bonus;
            }

            if (BitOperations.PopCount(Boards[Constants.WHITE_BISHOP]) >= 2)
                bonus += 66;

            if (BitOperations.PopCount(Boards[Constants.WHITE_KNIGHT]) >= 2)
                bonus -= 64;

            if (BitOperations.PopCount(Boards[Constants.WHITE_ROOK]) >= 2)
                bonus -= 100;

            if (BitOperations.PopCount(Boards[Constants.WHITE_QUEEN]) >= 2)
                bonus -= 180;

            if (BitOperations.PopCount(Boards[Constants.WHITE_PAWN]) == 0)
                bonus -= 300;

            return bonus;
        }

        public int Evaluate()
        {
            int materialScore = 0;
            int psScore = 0;

            for (int i = Constants.WHITE_PAWN; i <= Constants.WHITE_KING; i++)
            {
                materialScore += (BitOperations.PopCount(Boards[i]) - BitOperations.PopCount(Boards[i + 6])) * Constants.Material[i];

                psScore += EvaluatePS(Boards[i], Constants.Piece_Square_Table[i], Constants.WHITE);
                psScore -= EvaluatePS(Boards[i + 6], Constants.Piece_Square_Table[i], Constants.BLACK);
            }

            return materialScore + psScore + CalculateBonus();
        }

        public (int score, Move? bestMove) Search(int depth, int alpha = -int.MaxValue, int beta = int.MaxValue)
        {
            if (depth == 0)
            {
                return (Evaluate(), null); // Leaf node: static evaluation
            }

            //var moves = MoveGeneration();
            var moves = MoveOrdering();

            // No legal moves: checkmate or stalemate
            if (moves.Count == 0)
            {
                if (IsBlackKingChecked() || IsWhiteKingChecked())
                {
                    return (-int.MaxValue, null); // Checkmate = large negative value
                }
                return (0, null); // Stalemate = draw
            }

            int bestScore = -int.MaxValue;
            Move? bestMove = null;

            foreach (var move in moves)
            {
                var clone = Clone();
                clone.MakeMove(move);

                // Negamax recursion with flipped alpha-beta window
                var (score, _) = clone.Search(depth - 1, -beta, -alpha);
                score = -score;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = new Move(move);

                    if (score > alpha)
                    {
                        alpha = score;
                    }
                }

                if (score >= beta)
                {
                    return (bestScore, bestMove); // Beta cutoff
                }
            }

            return (bestScore, bestMove);
        }

        public List<Move> MoveOrdering()
        {
            return MoveGeneration().OrderByDescending(move => ScoreMove(move)).ToList();
        }

        private int ScoreMove(Move move)
        {
            // Score constants
            const int CAPTURE_BASE = 100000;
            const int PROMOTION_BASE = 50000;
            const int CASTLING_SCORE = 1000;

            // 1. Handle castling
            if (move.Castling != 0)
                return CASTLING_SCORE;

            // 2. Handle captures using MVV-LVA
            int victim = Util.GetPieceAtIndex(move.ToIndex, this);
            if (victim != -1)
            {
                int attackerValue = Constants.Material[move.FromBoard >= 6 ? move.FromBoard - 6 : move.FromBoard];
                int victimValue = Constants.Material[victim >= 6 ? victim - 6 : victim];
                return CAPTURE_BASE + (victimValue * 10 - attackerValue);
            }

            /*
             * 2.5 Special case: en passant. We handle them separately (although still MVV-LVA) because: 
             * 1. En passant is the only move where victim didn't stay in ToIndex
             * 2. En passant attacker and victim are always pawns
             */
            if ((move.FromBoard == Constants.WHITE_PAWN || move.FromBoard == Constants.BLACK_PAWN) && move.ToIndex == EnPassantTarget)
            {
                return CAPTURE_BASE + 9 * Constants.Material[Constants.WHITE_PAWN];
            }

            // 3. Handle promotions
            if (move.FromBoard != move.ToBoard)
            {
                int promotedValue = Constants.Material[move.ToBoard >= 6 ? move.ToBoard - 6 : move.ToBoard];
                return PROMOTION_BASE + promotedValue;
            }

            // 4. Quiet move
            return 0;
        }

    }
}
