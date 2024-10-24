using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Console;
using NAudio.Wave;

namespace dror_memory_game_vers_8._9
{
    internal class Program
    {
        static Random rnd = new Random();

        static int boardLeft = 0, boardTop = 1, previusCardIdx, currentCardIdx = 0, identicalCards = 2, uniqueCards = 5,
            marginX = 20 / (int)Math.Sqrt(identicalCards * uniqueCards), marginY = (int)(marginX / 2.2), cheatMargin = 5;
        static bool isCheatMode = false, playTypeSound = false, isBoardGeneric = false;

        static string musicPath = System.AppDomain.CurrentDomain.BaseDirectory;

        const int  closedCardInternalMargin = 0, openCardInternalMargin = 2, boardHeight = 30, boardWidth = boardHeight * 2;

        const string closedStr = "X", flipCardFileName = "flip_card.wav", typeFileName = "keyboard_typing.mp3";

        const ConsoleColor myCursorFrameForeColor = ConsoleColor.White, myCursorContantBackColor = ConsoleColor.Gray, closedCardForeColor = ConsoleColor.White
            , closedCardBackColor = ConsoleColor.Black, openCardBackgroundColor = ConsoleColor.Black;

        const ConsoleColor cocvitColor = ConsoleColor.Red, menuCocvitPlaceColor = ConsoleColor.Blue, menuOptionColor = ConsoleColor.White
            , minitextColor = ConsoleColor.Yellow;

        static void Main(string[] args)
        {
            Console.SetWindowSize(20, 10);
            while(Console.WindowHeight < boardHeight || Console.WindowWidth < boardWidth * 2)
            {
                WriteLine("Please make window bigger!");
                Console.Clear();
            }
            InitStartGame();

        }


        static void InitStartGame()
        {
            bool isRobotMode = false;
            int speed = 0;
            string[] boards = { "Generic board", "Numbers Board" };
            string[] Modes = { "Robot Mode", "Normal Mode - press [space] mid game for cheats" };

            isBoardGeneric = GetMenuInput(boards, cocvitColor, menuCocvitPlaceColor, menuOptionColor, minitextColor) == 0;
            Stopfor(300);
            Clear();
            isRobotMode = GetMenuInput(Modes, cocvitColor, menuCocvitPlaceColor, menuOptionColor, minitextColor) == 0;
            Stopfor(300);
            Clear();

            boardTop = 3;
            boardLeft = 5;

            CursorVisible = true;



            if (isBoardGeneric)
            {
                uniqueCards = 26;
                identicalCards = IntReadlineCheck("Enter number of times each card apears (2 for pairs, 3 for triplates...): ", 2, 110/uniqueCards);
            }
            else
            {
                identicalCards = IntReadlineCheck("Enter number of times each card apears (2 for pairs, 3 for triplates...): ", 2, 110 / uniqueCards);
                Clear();
                uniqueCards = IntReadlineCheck("Enter number of card values (number of card types): ", 2, 110 / identicalCards);
            }
            Clear();

            if(isRobotMode)
            {
                speed = 11 - IntReadlineCheck("Enter robot solving speed (1 - 10): ", 1, 10);
                Clear();
            }

            int cards = uniqueCards * identicalCards;

            int[] gameBoard = new int[cards];
            int[] stateBoard = new int[cards];

            InitStateBoard(stateBoard, 0);
            //Array.Clear(stateBoard, 0, stateBoard.Length);

            int cardsInARow = (int)Math.Ceiling(Math.Sqrt(cards));
            int cardWidth = (int)(boardWidth * 0.7 / cardsInARow);
            int cardHeight = (int)(boardHeight * 0.80 / ((cards - 1) / cardsInARow + 1.0));  //(int)((cardWidth + marginX )/ 1.8 ) - marginY + (int)(2.3 / Math.Sqrt(Math.Sqrt(uniqueCards *identicalCards)));

            marginX = (int)(boardWidth / cardsInARow) - cardWidth;
            marginY = (int)(boardHeight / ((cards - 1) / cardsInARow + 1.0)) - cardHeight;

            InitGameBoard(gameBoard);
            PrintGameBoard(true, gameBoard, stateBoard, cardsInARow, cardWidth, cardHeight);

            StartGame(gameBoard, stateBoard, isRobotMode, speed, cardsInARow, cardWidth, cardHeight);
        }

        static void StartGame(int[] gameBoard, int[] stateBoard, bool isRobotMode, int speed, int cardsInARow, int cardWidth, int cardHeight)
        {
            CursorVisible = false;


            List<int> openGuessCardsIdxs = new List<int>();
            openGuessCardsIdxs.Clear();

            List<int> robotIdxScedual = new List<int>();
            List<int>[] valuesApereances = new List<int>[gameBoard.Length / 2];
            List<int> robotUnknowenIndexes = new List<int>();
            InitIdxsList(robotUnknowenIndexes, gameBoard.Length);
            InitListsArray(valuesApereances);

            SetCursorPositionByIndex(0, cardsInARow, cardWidth, cardHeight);
            PrintCardContant(stateBoard[0], gameBoard[0], true, CursorLeft, CursorTop, cardWidth, cardHeight);

            int chosenCardIndex = 0, moves = 1;


            while (stateBoard.Contains(0)/* || isRobotMode && robotUnknowenIndexes.Count() != 0*/)
            {
                if (isRobotMode)
                    chosenCardIndex = RobotChosesCard(robotIdxScedual, valuesApereances, openGuessCardsIdxs, robotUnknowenIndexes, gameBoard, stateBoard,speed, cardsInARow, cardWidth, cardHeight);
                else
                    chosenCardIndex = ChooseCard(stateBoard, gameBoard, chosenCardIndex, cardsInARow, gameBoard.Length, cardWidth, cardHeight);
                stateBoard[chosenCardIndex] = 1;
                PrintMoves(moves, boardLeft, boardTop - 1);

                if (openGuessCardsIdxs.Count == identicalCards)//if 2 cards open
                {
                    moves++;

                    if (EqualValues(gameBoard, openGuessCardsIdxs) < openGuessCardsIdxs.Count)//if not all open guess cards are equal !allValuesAreEqual(openGuessCardsIdxs)
                    {
                        CloseCards(stateBoard, openGuessCardsIdxs, cardWidth, cardHeight, cardsInARow);
                        SetCursorPositionByIndex(chosenCardIndex, cardsInARow, cardWidth, cardHeight);
                    }
                    else
                    {
                        valuesApereances[gameBoard[openGuessCardsIdxs[0]]].Clear();
                    }
                    openGuessCardsIdxs.Clear();
                }

                openGuessCardsIdxs.Add(chosenCardIndex);

            }

            SetCursorPositionByIndex(chosenCardIndex, cardsInARow, cardWidth, cardHeight);
            PrintCardContant(stateBoard[chosenCardIndex], gameBoard[chosenCardIndex], false, CursorLeft, CursorTop, cardWidth, cardHeight);

            SetCursorPosition(0, boardTop + boardHeight + 3);
            if(isRobotMode)
            {
                WriteLine($"I have done it in  only " + moves + " moves!");
            }
            else
            {
                WriteLine($"You did it in " + moves + " moves!");
            }
            press();
            Clear();
            InitStartGame();
        }

        static int RobotChosesCard(List<int> robotIdxScedual, List<int>[] valuesApereances, List<int> openGuessCardsIdxs, List<int> robotUnknowenIndexes, int[] originalGameBoard, int[] stateBoard, int speed,int cardsInARow, int cardWidth, int cardHeight)
        {
            bool randomizIndex, allOpenIdentical = true;
            int indexToGoTo;
            int unknounIdx = 0, indexOfUnknownIdx = 0;
            if (robotIdxScedual.Count() == 0)
            {
                randomizIndex = true;
                allOpenIdentical = EqualValues(originalGameBoard, openGuessCardsIdxs) == openGuessCardsIdxs.Count;

                if (allOpenIdentical && openGuessCardsIdxs.Count == identicalCards)
                {
                    valuesApereances[originalGameBoard[openGuessCardsIdxs[0]]].Clear();
                }

                if (allOpenIdentical && openGuessCardsIdxs.Count != 0 && valuesApereances[originalGameBoard[openGuessCardsIdxs[0]]].Count == identicalCards)// all open cards are equal && all cards with their value  has apeard 
                {
                    for (int i = 0; i < identicalCards - openGuessCardsIdxs.Count; i++)
                    {
                        robotIdxScedual.Add(valuesApereances[originalGameBoard[openGuessCardsIdxs[0]]][i]);
                    }
                    valuesApereances[originalGameBoard[openGuessCardsIdxs[0]]].Clear();
                    randomizIndex = false;

                }
                else if (openGuessCardsIdxs.Count == identicalCards)//firstGuessCard
                {
                    foreach (List<int> aperiancesOfValue in valuesApereances)
                    {
                        if (aperiancesOfValue != null && aperiancesOfValue.Count == identicalCards)
                        {
                            foreach (int aperianceOfValue in aperiancesOfValue)
                            {
                                robotIdxScedual.Add(aperianceOfValue);
                            }
                            aperiancesOfValue.Clear();
                            randomizIndex = false;
                        }
                    }
                }

                if (robotUnknowenIndexes.Count == 0)
                {
                    robotIdxScedual.Add(Array.IndexOf(stateBoard, 0));
                }
                else if (randomizIndex)
                {
                    indexOfUnknownIdx = rnd.Next(robotUnknowenIndexes.Count);
                    unknounIdx = robotUnknowenIndexes[indexOfUnknownIdx];
                    robotUnknowenIndexes.RemoveAt(indexOfUnknownIdx);//remove the value - the index from the list of indexes

                    valuesApereances[originalGameBoard[unknounIdx]].Add(unknounIdx);//add place of color to memory(valuesApereances)
                    robotIdxScedual.Add(unknounIdx);

                }
            }

            indexToGoTo = robotIdxScedual[0];
            GoFromIdxToAnother(originalGameBoard, stateBoard, currentCardIdx, indexToGoTo, speed, cardsInARow, cardWidth, cardHeight);

            PrintApereances(valuesApereances, originalGameBoard, cardsInARow * (marginX + cardWidth) + 10, boardTop);

            robotIdxScedual.RemoveAt(0);

            return indexToGoTo;
        }

        static void CloseCards(int[] stateBoard, List<int> openGuessCardsIdxs, int cardWidth, int cardHeight, int cardsInARow)
        {
            for (int i = 0; i < openGuessCardsIdxs.Count; i++)
            {
                SetCursorPositionByIndex(openGuessCardsIdxs[i], cardsInARow, cardWidth, cardHeight);
                stateBoard[openGuessCardsIdxs[i]] = 0;
                PrintCard(0, 0, false, cardWidth, cardHeight);
            }
        }

        static void SetCursorPositionByIndex(int index, int cardsInARow, int cardWidth, int cardHeight)
        {
            SetCursorPosition(boardLeft + (index % cardsInARow) * (cardWidth + marginX), boardTop + (index / cardsInARow) * (cardHeight + marginY));
        }

        static void PrintGameBoard(bool sounds, int[] gameBoard, int[] stateBoard, int cardsInARow, int cardWidth, int cardHeight)
        {
            for (int i = 0; i < gameBoard.Length; i++)
            {
                SetCursorPositionByIndex(i, cardsInARow, cardWidth, cardHeight);
                PrintCard(stateBoard[i], gameBoard[i], false, cardWidth, cardHeight);
                if (sounds && gameBoard.Length <= 30)
                {
                    playSound(musicPath + flipCardFileName);
                    Thread.Sleep(200);
                }

            }
        }

        static int ChooseCard(int[] stateBoard, int[] gameBoard, int currentCardIdxGetter, int cardsInARow, int cards, int cardWidth, int cardHeight)
        {
            System.ConsoleKey key = ConsoleKey.H;
            ConsoleKeyInfo keyInfo;
            bool aCardWasChosen = false;

            currentCardIdx = currentCardIdxGetter;


            while (!aCardWasChosen)
            {
                while (KeyAvailable == false) { Stopfor(1); }

                keyInfo = ReadKey(true);
                key = keyInfo.Key;
                previusCardIdx = currentCardIdx;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (currentCardIdx - cardsInARow >= 0)
                        {
                            currentCardIdx -= cardsInARow;
                            //Beep(400, 50);
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        if (currentCardIdx + cardsInARow < cards)
                        {
                            currentCardIdx += cardsInARow;
                            //Beep(400, 50);
                        }
                        break;


                    case ConsoleKey.LeftArrow:
                        if (!(currentCardIdx % cardsInARow == 0))
                        {
                            currentCardIdx--;
                            //Beep(400, 50);
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (!((currentCardIdx + 1) % cardsInARow == 0) && currentCardIdx != gameBoard.Length - 1)
                        {
                            currentCardIdx++;
                            //Beep(400, 50);
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (stateBoard[currentCardIdx] == 0)
                        {
                            aCardWasChosen = true;
                            Beep(600, 50);
                        }
                        break;
                    case ConsoleKey.Spacebar:

                        int[] cheatStateBoard = new int[stateBoard.Length];

                        if (isCheatMode)
                        {
                            boardLeft += cardsInARow * (marginX + cardWidth) + cheatMargin;//direct right
                            SetCursorPosition(boardLeft, boardTop);//go right
                            PrintBlankBoard(gameBoard, cardsInARow, cardWidth, cardHeight);//cover cheat board
                            boardLeft -= cardsInARow * (marginX + cardWidth) + cheatMargin;//direct left
                            SetCursorPosition(boardLeft, boardTop);//go left
                            isCheatMode = false;
                        }
                        else
                        {
                            InitStateBoard(cheatStateBoard, 1);
                            boardLeft += cardsInARow * (marginX + cardWidth) + cheatMargin;//direct right
                            SetCursorPosition(boardLeft, boardTop);//go right
                            PrintGameBoard(false, gameBoard, cheatStateBoard, cardsInARow, cardWidth, cardHeight);//print cheat board
                            boardLeft -= cardsInARow * (marginX + cardWidth) + cheatMargin;//direct left
                            SetCursorPosition(boardLeft, boardTop);//go left
                            isCheatMode = true;
                        }
                        break;


                }

                MoveCursor(gameBoard, stateBoard, cardsInARow, cardWidth, cardHeight);

            }

            PrintCard(1, gameBoard[currentCardIdx], false, cardWidth, cardHeight);

            return currentCardIdx;
        }

        static void PrintCard(int state, int value, bool isCurser, int cardWidth, int cardHeight)
        {
            PrintCardFrame(state, value, isCurser, CursorLeft, CursorTop, cardWidth, cardHeight);
            PrintCardContant(state, value, isCurser, CursorLeft, CursorTop, cardWidth, cardHeight);
        }

        static void PrintCardContant(int state, int cardValue, bool isCursor, int cardLeft, int cardTop, int cardWidth, int cardHeight)
        {
            string internalStr = "";
            int internalMargin = 0;

            if (state == 1)
                internalMargin = openCardInternalMargin;
            else
                internalMargin = closedCardInternalMargin;

            ConsoleColor foreContantColor, backContantColor;

            if (state == 1)
            {
                if(isBoardGeneric)
                {
                    internalStr = ((char)(cardValue + 65)).ToString();
                }
                else
                {
                    internalStr = cardValue.ToString();
                }
                internalMargin = 1;
                foreContantColor = GetCardForeColor(cardValue);
                backContantColor = openCardBackgroundColor;
            }
            else
            {
                internalStr = closedStr;
                internalMargin = 0;
                foreContantColor = closedCardForeColor;
                backContantColor = closedCardBackColor;
            }

            if (isCursor)
            {
                backContantColor = myCursorContantBackColor;
            }

            ForegroundColor = foreContantColor;
            BackgroundColor = backContantColor;

            SetCursorPosition(cardLeft + 1, cardTop + 1);


            for (int i = 1; i < cardHeight - 1; i++)
            {
                for (int j = 1; j < cardWidth - 1; j++)
                {
                    if ((j - 1) % (internalMargin + internalStr.Length) == 0 && internalMargin + internalStr.Length + (j - 1) != cardWidth)
                    {
                        Write(internalStr);
                        j += internalStr.Length - 1;
                    }
                    else
                    {
                        for (int k = 0; k < internalMargin; k++)
                        {
                            Write(" ");
                        }
                    }

                }
                SetCursorPosition(CursorLeft - (cardWidth - 2), CursorTop + 1);
            }

            ForegroundColor = closedCardForeColor;
            BackgroundColor = openCardBackgroundColor;
        }

        static void PrintCardFrame(int state, int cardValue, bool isCursor, int cardLeft, int cardTop, int cardWidth, int cardHeight)
        {
            ConsoleColor foreFrameColor, backFrameColor;

            if (state == 1)
            {
                foreFrameColor = GetCardForeColor(cardValue);
                backFrameColor = openCardBackgroundColor;
            }
            else
            {
                foreFrameColor = closedCardForeColor;
                backFrameColor = closedCardBackColor;
            }

            if (isCursor)
            {
                foreFrameColor = myCursorFrameForeColor;
            }

            ForegroundColor = foreFrameColor;
            BackgroundColor = backFrameColor;

            for (int i = 1; i < cardHeight - 1; i++)
            {
                SetCursorPosition(cardLeft, cardTop + i);
                Write("│");
                SetCursorPosition(cardLeft + cardWidth - 1, cardTop + i);
                Write("│");
            }
            for (int i = 1; i < cardWidth - 1; i++)
            {
                SetCursorPosition(cardLeft + i, cardTop);
                Write("─");
                SetCursorPosition(cardLeft + i, cardTop + cardHeight - 1);
                Write("─");
            }
            SetCursorPosition(cardLeft, cardTop);
            Write("┌");
            SetCursorPosition(cardLeft, cardTop + cardHeight - 1);
            Write("└");
            SetCursorPosition(cardLeft + cardWidth - 1, cardTop);
            Write("┐");
            SetCursorPosition(cardLeft + cardWidth - 1, cardTop + cardHeight - 1);
            Write("┘");
            SetCursorPosition(cardLeft, cardTop);

            ForegroundColor = closedCardForeColor;
            BackgroundColor = openCardBackgroundColor;
        }

        static ConsoleColor GetCardForeColor(int cardContant)
        {
            ConsoleColor[] colors = {ConsoleColor.Red, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.DarkMagenta
               ,   ConsoleColor.Yellow, ConsoleColor.DarkBlue, ConsoleColor.Blue, ConsoleColor.DarkRed, ConsoleColor.Green
               ,   ConsoleColor.DarkYellow };

            return colors[cardContant % 10];
        }

        static void InitIdxsList(List<int> emptyIdxs, int cards)
        {
            emptyIdxs.Clear();
            for (int i = 0; i < cards; i++)
            {
                emptyIdxs.Add(i);
            }
        }

        static void InitGameBoard(int[] gameBoard)
        {
            List<int> emptyIndexes = new List<int>();
            int randomEmptyIndex = 0;

            InitIdxsList(emptyIndexes, gameBoard.Length);

            for (int i = 0; i < gameBoard.Length; i++)
            {
                randomEmptyIndex = emptyIndexes[rnd.Next(0, emptyIndexes.Count)];
                gameBoard[randomEmptyIndex] = i / identicalCards;
                emptyIndexes.Remove(randomEmptyIndex);
            }

        }

        static void InitStateBoard(int[] stateBoard, int state)
        {
            for (int i = 0; i < stateBoard.Length; i++)
            {
                stateBoard[i] = state;
            }

        }

        static void Stopfor(int sleepDuration)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < sleepDuration)
            {
                if (KeyAvailable)
                {
                    ReadKey(true);
                }
                Thread.Sleep(10);
            }
        }

        static int EqualValues(int[] gameBoard, List<int> idxList)
        {
            int equalValues = 0;
            for (int i = 0; i < idxList.Count && i == equalValues; i++)
            {
                if (gameBoard[idxList[0]] == gameBoard[idxList[i]])
                {
                    equalValues++;
                }
            }

            return equalValues;
        }

        static void PrintBlankBoard(int[] gameBoard, int cardsInARow, int cardWidth, int cardHeight)
        {
            for (int i = 0; i < gameBoard.Length; i++)
            {
                SetCursorPositionByIndex(i, cardsInARow, cardWidth, cardHeight);
                for (int j = 0; j < cardHeight; j++)
                {
                    for (int k = 0; k < cardWidth; k++)
                    {
                        Write(" ");
                    }
                    SetCursorPosition(CursorLeft - cardWidth, CursorTop + 1);
                }
            }
        }

        static void GoFromIdxToAnother(int[] gameBoard, int[] stateBoard, int initialIdx, int taragetIdx, int speed, int cardsInARow, int cardWidth, int cardHeight)
        {
            previusCardIdx = initialIdx;
            int initialCardRow = initialIdx / cardsInARow + 1, initialCardCol = (initialIdx) % cardsInARow + 1;
            int taragetCardRow = taragetIdx / cardsInARow + 1, taragetCardCol = (taragetIdx) % cardsInARow + 1;
            int moveCursorTime = 20 * speed, showCardTime = 80 * speed;


            if (initialCardRow == (gameBoard.Length - 1) / cardsInARow + 1)//if last row
            {
                for (int i = 0; i < Math.Abs(taragetCardRow - initialCardRow); i++)
                {
                    previusCardIdx = currentCardIdx;
                    Stopfor(moveCursorTime);
                    currentCardIdx += cardsInARow * (taragetCardRow - initialCardRow) / Math.Abs(taragetCardRow - initialCardRow); //1 or -1
                    MoveCursor(gameBoard, stateBoard, cardsInARow, cardWidth, cardHeight);
                }

                for (int i = 0; i < Math.Abs(taragetCardCol - initialCardCol); i++)
                {
                    previusCardIdx = currentCardIdx;
                    Stopfor(moveCursorTime);
                    currentCardIdx += (taragetCardCol - initialCardCol) / Math.Abs(taragetCardCol - initialCardCol); //1 or -1
                    MoveCursor(gameBoard, stateBoard, cardsInARow, cardWidth, cardHeight);
                }


            }
            else
            {
                for (int i = 0; i < Math.Abs(taragetCardCol - initialCardCol); i++)
                {
                    previusCardIdx = currentCardIdx;
                    Stopfor(moveCursorTime);
                    currentCardIdx += (taragetCardCol - initialCardCol) / Math.Abs(taragetCardCol - initialCardCol); //1 or -1
                    MoveCursor(gameBoard, stateBoard, cardsInARow, cardWidth, cardHeight);
                }

                for (int i = 0; i < Math.Abs(taragetCardRow - initialCardRow); i++)
                {
                    previusCardIdx = currentCardIdx;
                    Stopfor(moveCursorTime);
                    currentCardIdx += cardsInARow * (taragetCardRow - initialCardRow) / Math.Abs(taragetCardRow - initialCardRow); //1 or -1
                    MoveCursor(gameBoard, stateBoard, cardsInARow, cardWidth, cardHeight);
                }
            }

            stateBoard[currentCardIdx] = 1;
            Beep(600, 20);
            Stopfor(showCardTime - 20);
        }

        static void MoveCursor(int[] gameBoard, int[] stateBoard, int cardsInARow, int cardWidth, int cardHeight)
        {

            SetCursorPositionByIndex(previusCardIdx, cardsInARow, cardWidth, cardHeight);

            PrintCard(stateBoard[previusCardIdx], gameBoard[previusCardIdx], false, cardWidth, cardHeight);

            SetCursorPositionByIndex(currentCardIdx, cardsInARow, cardWidth, cardHeight);


            PrintCard(stateBoard[currentCardIdx], gameBoard[currentCardIdx], true, cardWidth, cardHeight);


            SetCursorPositionByIndex(currentCardIdx, cardsInARow, cardWidth, cardHeight);


        }

        static void InitListsArray(List<int>[] listsArray)
        {
            for (int i = 0; i < listsArray.Length; i++)
            {
                listsArray[i] = new List<int>();
            }
        }

        static void PrintApereances(List<int>[] valuesApereances, int[] gameBoard, int left, int top)
        {
            int origTop = CursorTop, origLeft = CursorLeft;

            SetCursorPosition(left, top + 3);

            Write("    Robot memory :");
            SetCursorPosition(left, top + 5);
            Write("value :    index apereances");

            for (int i = 0; i < valuesApereances.Length; i++)
            {
                if (valuesApereances[i].Count > 0)
                {
                    SetCursorPosition(left + (i / 20) * (identicalCards * (3 + (uniqueCards * 2).ToString().Length) + 7), top + i % 20 + 6);

                    Write(gameBoard[valuesApereances[i][0]] + ":   ");//
                    for (int j = 0; j < valuesApereances[i].Count(); j++)
                    {
                        Write(valuesApereances[i][j] + " , ");
                    }

                }

            }

            SetCursorPosition(origLeft, origTop);
        }

        static void PrintMoves(int moves, int left, int top)
        {
            int origTop = CursorTop, origLeft = CursorLeft;

            SetCursorPosition(left, top);
            Write("moves: " + moves);
            SetCursorPosition(origLeft, origTop);
        }

        static int GetMenuInput(string[] options, ConsoleColor cocvitColor = ConsoleColor.Green, ConsoleColor menuCocvitPlaceColor = ConsoleColor.Blue, ConsoleColor menuOptionColor = ConsoleColor.Magenta, ConsoleColor minitextColor = ConsoleColor.Yellow, bool doTypeMinitext = true)
        {
            //string[] Modes = { "h", "gd", "s", "a" };
            //GetMenuInput(Modes, cocvitColor, menuCocvitPlaceColor, menuOptionColor, minitextColor);

            WriteLine();
            int origTop = CursorTop;
            PrintMenu(options, cocvitColor, menuCocvitPlaceColor, menuOptionColor, minitextColor, doTypeMinitext = true);

            int howmanylines;
            howmanylines = options.Length;
            int res = 9;
            ConsoleKey key = ConsoleKey.H;
            SetCursorPosition(2, origTop);
            colorPrint("*", cocvitColor);
            SetCursorPosition(2, origTop);
            ConsoleKeyInfo cki;
            while (key != ConsoleKey.Enter)
            {
                while (KeyAvailable == false) { Stopfor(1); }

                cki = ReadKey(true);
                key = cki.Key;
                if (key == ConsoleKey.DownArrow & CursorTop != (origTop + (howmanylines - 1) * 2))
                {
                    SetCursorPosition(2, CursorTop); Write(" "); SetCursorPosition(2, CursorTop + 2); colorPrint("*", cocvitColor); Beep(400, 50); ;
                    ResetColor(); SetCursorPosition(2, CursorTop);
                }
                else if (key == ConsoleKey.UpArrow & CursorTop != origTop) { SetCursorPosition(2, CursorTop); Write(" "); SetCursorPosition(2, CursorTop - 2); colorPrint("*", cocvitColor); SetCursorPosition(2, CursorTop); Beep(400, 50); }
            }
            res = (CursorTop - origTop) / 2;
            Beep(600, 50);

            SetCursorPosition(0, origTop + (options.Length + 1) * 2);

            return res;

        }

        static void PrintMenu(string[] options, ConsoleColor cocvitColor = ConsoleColor.Green, ConsoleColor menuCocvitPlaceColor = ConsoleColor.Blue, ConsoleColor menuOptionColor = ConsoleColor.Magenta, ConsoleColor minitextColor = ConsoleColor.Yellow, bool typeTheMinitext = true)
        {
            for (int i = 0; i < options.Length; i++) { colorPrint(" [ ]  ", menuCocvitPlaceColor); colorPrint(options[i], menuOptionColor); WriteLine("\n"); }


            Stopfor(200);
            if (typeTheMinitext) { ColorType(" move the ", minitextColor, 1); colorPrint("*", cocvitColor); ColorType(" by pressing up and down keys and press enter to lock\n", minitextColor, 15); }
            else { colorPrint(" move the ", minitextColor); colorPrint("*", cocvitColor); colorPrint(" by pressing up and down keys and press enter to lock\n", minitextColor); }

        }

        static void SlowType(string text, int time = 30)
        {
            playTypeMusic(musicPath + typeFileName);
            for (int i = 0; i < text.Length; i++) { Write(text[i]); Stopfor(time); }
            playTypeSound = false;
        }

        static void colorPrint(string text, ConsoleColor color)
        {
            ForegroundColor = color;
            Write(text);
            ForegroundColor = ConsoleColor.White;
        }

        static void ColorType(string text, ConsoleColor color, int fw)
        {
            ForegroundColor = color;
            SlowType(text);
            ForegroundColor = ConsoleColor.White;
        }

        public static void playSound(string path)
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = path;
            player.Play();
        }
        public static int IntReadlineCheck(string Question, int min , int max = 2147483647)
        {
            string strInput;// The input as a string
            bool checkingLoop;//The loop that repet the Question if the input is possible
            int IntChecker;// an int variable used to chack parse to int
            int intInput = 0;//
            int origTop, origLeft;
            SlowType(Question);
            do
            {//do the code part no matter what
                origLeft = CursorLeft; origTop = CursorTop;
                strInput = ReadLine();
                if (int.TryParse(strInput, out IntChecker))//if the convertion of the string to an int veriable sucseeded
                {
                    intInput = int.Parse(strInput);
                    checkingLoop = false;
                    if(intInput > max || intInput < min)
                    {
                        Write("You need to enter a whole number that is smaller than " + (max + 1) +" and bigger than " + (min - 1));
                        Delete(origTop, origLeft, strInput);
                        checkingLoop = true;
                    }
                }
                else 
                {
                    Write("You need to enter a whole number that is smaller than " + (max + 1));
                    checkingLoop = true;
                    Delete(origTop, origLeft, strInput);
                }

            } while (checkingLoop); //repet if needed
            return intInput;
        }

        /*
         this function plays the first X ~10ths of a second  for a loop until "playBackgroundMusic" is set to false
            using: Naudio library - .wav & System.Threading.Tasks;
            input: the path of the mp3 file
            output: none
         */
        private static async Task playTypeMusic(string mp3FilePath)
        {
            playTypeSound = true;
            while (playTypeSound)
            {
                using (var reader = new Mp3FileReader(mp3FilePath))
                using (var waveOut = new WaveOutEvent())
                {
                    waveOut.Init(reader);
                    waveOut.Play();
                    int i = 0;
                    while (playTypeSound & i < 150)// the time of one round of loop (in values of 100 miliseconds a second)
                    {
                        await Task.Delay(100);//100 miliseconds
                        i++;
                    }


                }
            }
        }

        public static void Delete(int origt, int origl, string userinput)
        {
            Stopfor(2000);
            int origl2 = (CursorLeft);
            SetCursorPosition(0, origt + 1);
            for (int i = 0; i < origl2; i++) { Write(" "); }
            SetCursorPosition(origl, origt);
            for (int i = 0; i < userinput.Length; i++) { Write(" "); }
            SetCursorPosition(origl, origt);
        }

        public static void press()
        {
            WriteLine("\nPress any key to start over..");
            while (KeyAvailable == false) { Stopfor(1); }
        }




    }
}