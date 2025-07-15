namespace Seven
{
    internal class Move
    {
        public int Castling { get; set; }
        public int FromBoard { get; set; }
        public int FromIndex { get; set; }
        public int ToBoard { get; set; }
        public int ToIndex { get; set; }

        public Move(int castling)
        {
            Castling = castling;
        }

        public Move(int from, int to, int fromIndex, int toIndex)
        {
            FromBoard = from;
            FromIndex = fromIndex;
            ToBoard = to;
            ToIndex = toIndex;
        }

        public Move(string move, Chess chess)
        {
            switch (move)
            {
                case "O-O":
                    Castling = Constants.WHITE_KING_SIDE;
                    break;
                case "O-O-O":
                    Castling = Constants.WHITE_QUEEN_SIDE;
                    break;
                case "o-o":
                    Castling = Constants.BLACK_KING_SIDE;
                    break;
                case "o-o-o":
                    Castling = Constants.BLACK_QUEEN_SIDE;
                    break;
                default:
                    {
                        FromIndex = Util.FromAlgebraicToIndex(move.Substring(0, 2));
                        ToIndex = Util.FromAlgebraicToIndex(move.Substring(2, 2));

                        for (int i = 0; i < chess.Boards.Length; i++)
                        {
                            if (Util.IsPieceAtIndex(chess.Boards[i], FromIndex))
                            {
                                FromBoard = i;
                                ToBoard = i;
                            }

                            //If this is promotion, recalculate ToBoard
                            if (move.Length == 5)
                            {
                                _ = move[4] switch
                                {
                                    'Q' => ToBoard = Constants.WHITE_QUEEN,
                                    'R' => ToBoard = Constants.WHITE_ROOK,
                                    'N' => ToBoard = Constants.WHITE_KNIGHT,
                                    'B' => ToBoard = Constants.WHITE_BISHOP,
                                    'q' => ToBoard = Constants.BLACK_QUEEN,
                                    'r' => ToBoard = Constants.BLACK_ROOK,
                                    'n' => ToBoard = Constants.BLACK_KNIGHT,
                                    'b' => ToBoard = Constants.BLACK_BISHOP,
                                    _ => throw new NotImplementedException(),
                                };
                            }
                        }
                        break;
                    }

            }
        }

        public Move(Move mv)
        {
            Castling = mv.Castling;
            FromBoard = mv.FromBoard;
            ToBoard = mv.ToBoard;
            FromIndex = mv.FromIndex;
            ToIndex = mv.ToIndex;
        }

        private string GetNormalMove()
        {
            var move = $"{Util.FromIndexToAlgebraic(FromIndex)}{Util.FromIndexToAlgebraic(ToIndex)}";
            //Only pawn promotion that FromBoard and ToBoard are different
            if (FromBoard != ToBoard)
            {
                if (ToBoard == 1 || ToBoard == 7)
                {
                    move += "r";
                }
                else if (ToBoard == 2 || ToBoard == 8)
                {
                    move += "n";
                }
                else if (ToBoard == 3 || ToBoard == 9)
                {
                    move += "b";
                }
                else if (ToBoard == 4 || ToBoard == 10)
                {
                    move += "q";
                }
            }
            return move;
        }

        public override string ToString()
        {
            var move = Castling switch
            {
                Constants.WHITE_KING_SIDE => "O-O",
                Constants.WHITE_QUEEN_SIDE => "O-O-O",
                Constants.BLACK_KING_SIDE => "o-o",
                Constants.BLACK_QUEEN_SIDE => "o-o-o",
                _ => GetNormalMove(),
            };
            return move;
        }
    }
}
