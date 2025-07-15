using Seven;

namespace Seven
{
    internal class Seven
    {
        public static void TestFEN(string[] fens, Chess chess)
        {
            foreach (var fen in fens)
            {
                chess.FEN(fen);
                Console.WriteLine($"FEN: {fen}");
                Console.WriteLine(chess);
                Console.WriteLine();
            }
        }

        public static void TestMoveGen(string[] tests, Chess chess)
        {
            List<Move> moves;
            foreach (var test in tests)
            {
                chess.FEN(test);
                moves = chess.MoveGeneration();
                Console.WriteLine($"Found {moves.Count} moves");
                foreach (var move in moves)
                {
                    Console.Write(move.ToString() + " ");
                }
                Console.WriteLine();
            }
        }

        public static void TestMove(string[] fens, Chess chess)
        {
            List<Move> moves;
            Chess clone;
            foreach (var fen in fens)
            {
                chess.FEN(fen);
                moves = chess.MoveGeneration();
                foreach (var move in moves)
                {
                    clone = chess.Clone();
                    Console.WriteLine($"{move}");
                    clone.MakeMove(move);
                    Console.WriteLine(clone + "\n");
                }
            }
        }

        public static void TestPerft(string[] tests, Chess chess, int depth)
        {
            string fen;
            for (int i = 0; i < tests.Length; i += 3)
            {
                //The first line is the FEN, second line is depth, and third line is an empty line for separation
                fen = tests[i];
                Console.WriteLine($"Testing position: {fen}");
                depth = int.Parse(tests[i + 1]);

                chess.FEN(fen);
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var results = chess.FastPerft(depth);
                stopwatch.Stop();
                var total = 0;
                foreach (var res in results)
                {
                    Console.WriteLine($"{res.Key}: {res.Value}");
                    total += res.Value;
                }
                Console.WriteLine($"Total node found: {total}");
                Console.WriteLine($"Test run successfully in {stopwatch.ElapsedMilliseconds} ms");
            }

        }

        public static void Test(string[] tests, Chess chess)
        {
            foreach (var test in tests)
            {
                chess.FEN(test);
                var moves = chess.WhiteKingMoves();
                foreach (var move in moves)
                {
                    Console.Write($"{move} ");
                }
                Console.WriteLine();
            }
        }

        public static void TestSearch(string fen, Chess chess, int depth)
        {
            chess.FEN(fen);
            Console.WriteLine($"Start searching with position {fen}");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var (_, bestMove) = chess.Search(depth);
            stopwatch.Stop();

            Console.WriteLine($"Found best move {bestMove} with depth {depth}");
            Console.WriteLine($"Task complete in {stopwatch.ElapsedMilliseconds} ms");
        }


        public static void Main(string[] args)
        {
            var chess = new Chess();

            if (args.Length > 0)
            {
                //Get test mode
                string testMode = args[0];
                switch (testMode)
                {
                    case "fen":
                        {
                            TestFEN(File.ReadAllLines("fen_input.txt"), chess);
                            break;
                        }
                    case "move_gen":
                        {
                            TestMoveGen(File.ReadAllLines("perft_input.txt"), chess);
                            break;
                        }
                    case "move":
                        {
                            TestMove(File.ReadAllLines("perft_input.txt"), chess);
                            break;
                        }
                    case "perft":
                        {
                            if (args.Length == 2)
                            {
                                int depth = int.Parse(args[1]);
                                TestPerft(File.ReadAllLines("perft_input.txt"), chess, depth);
                            }
                            break;
                        }
                    case "search":
                        {
                            if (args.Length == 2)
                            {
                                int depth = int.Parse(args[1]);
                                TestSearch(File.ReadAllLines("perft_input.txt")[0], chess, depth);
                            }
                            break;
                        }
                    case "test":
                        {
                            Test(File.ReadAllLines("perft_input.txt"), chess);
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Not supported argument");
                            break;
                        }
                }

            }
        }
    }
}