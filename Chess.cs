using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Net.Mime.MediaTypeNames;

namespace Chess
{
    public class Chess : Game
    {
        //Texture2D ballTexture;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Point GameBounds = new Point(720, 720); // Game resolutions
        private int tileWidth;
        const int Padding = 20;
        private bool PieceIsSelected = false;
        private string[] pieces = [ "king", "queen", "bishop", "knight", "rook", "pawn"];

        private bool isDraging = false;

        private Piece slectedPiece;

        private Dictionary<string, Texture2D> pieceTextures = new(){};

        private Byte[] board = new byte[64];

        private Texture2D whitedot;

        private string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        private int colorTurn = 0;

        private List<int> possibleMoveLocations ;




        public Texture2D Texture;

        private enum Side { white, black }

        private class Piece {
            public int value { get; set; }
            public int currentBoardLocation { get; set; }
        }


        public Chess()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = GameBounds.X;
            _graphics.PreferredBackBufferHeight = GameBounds.Y;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            tileWidth = (GameBounds.X - Padding * 2) / 8;
        }


        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            if (Texture == null)
            {   //create texture to draw with if it does not exist
                Texture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
                Texture.SetData<Color>(new Color[] { Color.White });
            }
            this.TranslateFen(this.fen, this.board);

            //this.allPieces = this.initilaizePieces(Side.white);
            //allPieces.AddRange(this.initilaizePieces(Side.black));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("MyMenuFont");

            foreach (string piece in this.pieces)
            {
                this.pieceTextures.Add($"white-{piece}", Content.Load<Texture2D>($"white-{piece}"));
                this.pieceTextures.Add($"black-{piece}", Content.Load<Texture2D>($"black-{piece}"));
            }
            this.whitedot = Content.Load<Texture2D>("whitedot");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
         
            MouseState msState = Mouse.GetState();
            var boardLoc = (msState.Y - Padding) / tileWidth * 8 + (msState.X - Padding) / tileWidth;
            if (msState.LeftButton == ButtonState.Pressed)
            {
               
                if (!this.isDraging)
                {
                    var selectePice = boardLoc >= 0 && boardLoc < 64 ? board[boardLoc] : 0;
                    if (selectePice != 0 && selectePice >> 6 ==  colorTurn)
                    {
                        this.slectedPiece = new Piece();
                        this.slectedPiece.currentBoardLocation = boardLoc;
                        
                        this.slectedPiece.value = selectePice;
                        this.isDraging = true;
                        var nextLocations = this.GetPossibleMoveLocations(this.slectedPiece.value, this.slectedPiece.currentBoardLocation, this.board);
                        nextLocations = CleanPossibleLocation(nextLocations, (byte)this.slectedPiece.value, this.slectedPiece.currentBoardLocation, this.board);
                        this.possibleMoveLocations = nextLocations;
                    }
                }
                if (this.slectedPiece != null)
                {
                    CompletMove(boardLoc);
                }
            }

            if (msState.LeftButton == ButtonState.Released)
            {
                if (this.isDraging)
                { 
                    CompletMove(boardLoc);
                    this.isDraging = false;
                }
            }


            base.Update(gameTime);
        }

        private List<int> CleanPossibleLocation(List<int> locations, byte pieceVal, int pieceLocation, byte[] boardToCheck)
        {
            byte[] tempBoard = new byte[64];
            
            List<int> tempLocations = new List<int>(locations); 
            foreach (int location in tempLocations)
            {
                Array.Copy(boardToCheck, tempBoard, 64);
                tempBoard[pieceLocation] = 0;
                tempBoard[location] = pieceVal;
                if (isCheck(tempBoard, this.colorTurn))
                {  
                    locations.Remove(location);
                }
            }
            return locations;
        }

        private void CompletMove( int boardLoc)
        {
            if (boardLoc >= 0 && boardLoc < 64 && this.possibleMoveLocations.Contains(boardLoc))
            {
                
                //if (boardLoc >= 0 && boardLoc < 64 )
                var boardCopy = this.board;
                boardCopy[this.slectedPiece.currentBoardLocation] = 0;
                board[this.slectedPiece.currentBoardLocation] = 0;
                board[boardLoc] = (byte)this.slectedPiece.value;
                this.slectedPiece.currentBoardLocation = boardLoc;
                this.colorTurn = colorTurn == 0 ? 1 : 0;
                this.possibleMoveLocations = [];
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();

          
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var iP = i + 1;
                    var jP = j + 1;
                    this.DrawRectangle(_spriteBatch, new Rectangle(Padding + tileWidth*j, Padding + tileWidth * i, tileWidth , tileWidth), (i+j) % 2  == 0 ? Color.Wheat : Color.BurlyWood);
                    
                    var boardInt = i * 8 + j;
                    if (board[boardInt] == 0 || boardInt > 64)
                    {
                       continue;
                    }
                    var cPiece = (board[boardInt] & (byte)0b00111111);
                    string currentPiece = this.pieces[cPiece - 1];
                    string currentPieceColor = board[boardInt] >> 6 == (byte)0b01 ? "black" : "white";
                    Texture2D texture = this.pieceTextures[$"{currentPieceColor}-{currentPiece}"];
                    if (this.isDraging && this.slectedPiece.currentBoardLocation == boardInt)
                    {
                        _spriteBatch.Draw(texture, new Rectangle(Mouse.GetState().X - tileWidth/2, Mouse.GetState().Y - tileWidth / 2, tileWidth, tileWidth), Color.Red);
                    }
                    else
                    {
                        _spriteBatch.Draw(texture, new Rectangle(Padding + (j * tileWidth), Padding + (i * tileWidth), tileWidth, tileWidth), Color.White);
                    }
                    
                }
            }

             // Draw the pieces
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var boardInt = i * 8 + j;
                    if (board[boardInt] == 0 || boardInt > 64)
                    {
                        continue;
                    }
                    var cPiece = (board[boardInt] & (byte)0b00111111);
                    string currentPiece = this.pieces[cPiece - 1];
                    string currentPieceColor = board[boardInt] >> 6 == (byte)0b01 ? "black" : "white";
                    Texture2D texture = this.pieceTextures[$"{currentPieceColor}-{currentPiece}"];
                    if (!this.isDraging)
                    {
                        _spriteBatch.Draw(texture, new Rectangle(Padding + (j * tileWidth), Padding + (i * tileWidth), tileWidth, tileWidth), this.slectedPiece?.currentBoardLocation == boardInt ? Color.Red : Color.White);
                       
                    }
                    else if(this.slectedPiece.currentBoardLocation == boardInt)
                    {
                        _spriteBatch.Draw(texture, new Rectangle(Mouse.GetState().X - tileWidth / 2, Mouse.GetState().Y - tileWidth / 2, tileWidth, tileWidth), Color.Red);
                    }
                }
            }

            if (this.slectedPiece != null)
            {
                this.DrawPosibbleMoves(_spriteBatch);
                

            }
            string output = isCheck(this.board, colorTurn) ? "Check" : "Not Check";
            output = isCheckmate(this.board, colorTurn) ? "Checkmate" : output;

            Vector2 FontOrigin = new Vector2(-Padding, Padding / 2);
            Vector2 fontPos = new Vector2(0, Padding / 2);
            _spriteBatch.DrawString(_spriteFont, output, fontPos, Color.DarkOrchid,
        0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);



            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawRectangle(SpriteBatch sb, Rectangle rec, Color color)
        {
            Vector2 pos = new Vector2(rec.X, rec.Y);

            sb.Draw(Texture, pos, rec,
                color * 1.0f,
                0, Vector2.Zero, 1.0f,
                SpriteEffects.None, 0.00001f);
        }

        private void DrawPosibbleMoves(SpriteBatch sb)
        {
            foreach (int location in this.possibleMoveLocations)
            {
                int Y = location / 8;
                int X = location % 8;
                sb.Draw(this.whitedot, new Rectangle(Padding + (X * tileWidth) + tileWidth / 4, Padding + (Y * tileWidth) + tileWidth / 4, tileWidth / 2, tileWidth / 2), Color.Gray);
            }
            
        }

        private void addToListOfPossibleLocationPawn(int loc, ref List<int> locations, byte[] newBoardToCheck)
        {
            if (newBoardToCheck[loc] == 0)
            {
                locations.Add(loc);
            }
        }

        private void addRookMovement(int row, int col, int pieceColor, int pieceLocation, ref List<int> locations, byte[] newBoardToCheck)
        {
            var newLoc = pieceLocation;

            for (int i = row + 1; i < 8; i++)
            {
                newLoc = i * 8 + col;
                if (newBoardToCheck[newLoc] != 0 && newBoardToCheck[newLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(newLoc, ref locations, newBoardToCheck);
                if (newBoardToCheck[newLoc] != 0 && newBoardToCheck[newLoc] >> 6 != pieceColor)
                {
                    locations.Add(newLoc);
                    break;
                }

            }
            
            for(int i = row - 1; i >= 0; i--)
            {
                newLoc = i * 8 + col;
                if (newBoardToCheck[newLoc] != 0 && newBoardToCheck[newLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(newLoc, ref locations, newBoardToCheck);
                if (newBoardToCheck[newLoc] != 0 && newBoardToCheck[newLoc] >> 6 != pieceColor)
                {
                    locations.Add(newLoc);
                    break;
                }

            }

            for (int i = col + 1; i < 8; i++)
            {
                newLoc = row * 8 + i;
                if (newBoardToCheck[newLoc] != 0 && newBoardToCheck[newLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(newLoc, ref locations, newBoardToCheck);
                if (newBoardToCheck[newLoc] != 0 && newBoardToCheck[newLoc] >> 6 != pieceColor)
                {
                    locations.Add(newLoc);
                    break;
                }

            }

            for (int i = col - 1; i >= 0; i--)
            {
                newLoc = row * 8 + i;
                if (newBoardToCheck[newLoc] != 0 && newBoardToCheck[newLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(newLoc, ref locations, newBoardToCheck);
                if (newBoardToCheck[newLoc] != 0 && newBoardToCheck[newLoc] >> 6 != pieceColor)
                {
                    locations.Add(newLoc);
                    break;
                }

            }

        }

        private void addBishopMovement(int row, int col, int pieceColor, int pieceLocation, ref List<int> locations, byte[] newBoardToCheck)
        {
            int nextLoc = pieceLocation;

            //first Diag positive side
            for (int i = col + 1; i < 8; i++)
            {
                nextLoc = (i) + (row - (col - i)) * 8;
                if (nextLoc > 63 || nextLoc < 0)
                    break;
                if (newBoardToCheck[nextLoc] != 0 && newBoardToCheck[nextLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(nextLoc, ref locations, newBoardToCheck);
                if (newBoardToCheck[nextLoc] != 0 && newBoardToCheck[nextLoc] >> 6 != pieceColor)
                {
                    locations.Add(nextLoc);
                    break;
                }
            }

            Console.Write("trdy00");
            for (int i = col - 1; i >= 0; i--)
            {
                nextLoc = (i) + (row + (col - i)) * 8;
                if (nextLoc > 63 || nextLoc < 0)
                    break;
                if (newBoardToCheck[nextLoc] != 0 && newBoardToCheck[nextLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(nextLoc, ref locations, newBoardToCheck);
                if (newBoardToCheck[nextLoc] != 0 && newBoardToCheck[nextLoc] >> 6 != pieceColor)
                {
                    locations.Add(nextLoc);
                    break;
                }
            }
            for (int i = col + 1; i < 8; i++)
            {
                nextLoc = (i) + (row + (col - i)) * 8;
                if (nextLoc > 63 || nextLoc < 0)
                    break;
                if (newBoardToCheck[nextLoc] != 0 && newBoardToCheck[nextLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(nextLoc, ref locations, newBoardToCheck);
                if (newBoardToCheck[nextLoc] != 0 && newBoardToCheck[nextLoc] >> 6 != pieceColor)
                {
                    locations.Add(nextLoc);
                    break;
                }
            }

            for (int i = col - 1; i >= 0; i--)
            {
                nextLoc = (i) + (row - (col - i)) * 8;
                if (nextLoc > 63 || nextLoc < 0)
                    break;
                if (newBoardToCheck[nextLoc] != 0 && newBoardToCheck[nextLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(nextLoc, ref locations, newBoardToCheck);
                if (newBoardToCheck[nextLoc] != 0 && newBoardToCheck[nextLoc] >> 6 != pieceColor)
                {
                    locations.Add(nextLoc);
                    break;
                }
            }
        }

        private bool checkBoardLocValid(int boardLoc)
        {
            if (boardLoc > 63 || boardLoc < 0)
            {
                return false;
            }
            return true;
        }

        private bool checkPossibleMoveToLocation(int boardLoc,int pieceColor, byte[] newBoardToCheck)
        {
            if (boardLoc > 63 || boardLoc < 0)
               return false;
            if (newBoardToCheck[boardLoc] != 0 && newBoardToCheck[boardLoc] >> 6 == pieceColor)
                return false;
            return true;
        }

        private List<int> GetPossibleMoveLocations(int piece, int pieceLocation, byte[] newBoardToCheck)
        {
            var locations = new List<int>();
            var pieceVal = piece & 0b00001111;
            var pieceColor = piece >> 6;
            var row = pieceLocation / 8;
            var col = pieceLocation % 8;

            //pawn
            if (pieceVal == (byte)0b0110 && pieceLocation/8 != 0 && pieceLocation / 8 != 7)
            {
                if(pieceLocation/8 == 1 && pieceColor == 1)
                {
                    this.addToListOfPossibleLocationPawn(pieceLocation+8, ref locations, newBoardToCheck);
                    if(locations.Count > 0)
                    this.addToListOfPossibleLocationPawn(pieceLocation+16, ref locations, newBoardToCheck);
                }
                else if (pieceLocation / 8 == 6 && pieceColor == 0)
                {
                    this.addToListOfPossibleLocationPawn(pieceLocation - 8, ref locations, newBoardToCheck);
                    if (locations.Count > 0)
                    this.addToListOfPossibleLocationPawn(pieceLocation - 16, ref locations, newBoardToCheck);
                }
                else
                {
                    var newLoc = pieceColor == 1 ? pieceLocation + 8 : pieceLocation - 8;
                    this.addToListOfPossibleLocationPawn(newLoc, ref locations, newBoardToCheck);
                    var captureLoc1 = pieceColor == 1 ? pieceLocation + 9 : pieceLocation - 9;
                    var captureLoc2 = pieceColor == 1 ? pieceLocation + 7 : pieceLocation - 7;
                    if (newBoardToCheck[captureLoc1] != 0  && captureLoc1 /8 != pieceLocation)
                    {
                        if (board[captureLoc1] >> 6 != pieceColor)
                            locations.Add(captureLoc1);
                    }
                    if (newBoardToCheck[captureLoc2] != 0 && captureLoc2 / 8 != pieceLocation / 8)
                    {
                        if (board[captureLoc1] >> 6 != pieceColor)
                            locations.Add(captureLoc2);
                    }
                }
            }

            // rook
            if(pieceVal == (byte)0b00000101)
            {
                this.addRookMovement(row, col, pieceColor, pieceLocation, ref locations, newBoardToCheck);
            }
            
            // bishop
            if(pieceVal == (byte)0b00000011)
            {
               
                this.addBishopMovement(row, col, pieceColor, pieceLocation, ref locations, newBoardToCheck);              
            }

            // queen
            if (pieceVal == (byte)0b00000010)
            {

                this.addBishopMovement(row, col, pieceColor, pieceLocation, ref locations, newBoardToCheck);
                this.addRookMovement(row, col, pieceColor, pieceLocation, ref locations, newBoardToCheck);
            }

            //  Knight
            if ((pieceVal == (byte)0b00000100))
            {
                var possibleNextLocs = new List<int>();
                
                //down 
                if(col < 6 && row  < 7)
                    possibleNextLocs.Add((row + 1) * 8 + col + 2);
                if (col < 7 && row < 6)
                    possibleNextLocs.Add((row + 2) * 8 + col + 1);

                //up
                if (col < 6 && row > 0)
                    possibleNextLocs.Add((row  - 1) * 8 + col + 2);
                if (col < 7 && row > 1)
                    possibleNextLocs.Add((row  - 2) * 8 + col + 1);
                
                if (col > 1 && row > 0)
                    possibleNextLocs.Add((row - 1) * 8 + col - 2);
                if (col > 0 && row > 1)
                    possibleNextLocs.Add((row - 2) * 8 + col - 1);

                //up
                if (col > 1 && row < 7)
                    possibleNextLocs.Add((row + 1) * 8 + col - 2);
                if (col > 0 && row < 6)
                    possibleNextLocs.Add((row + 2) * 8 + col - 1);

                foreach(int loc in possibleNextLocs)
                {
                    if(this.checkPossibleMoveToLocation(loc, pieceColor,newBoardToCheck))
                    {
                        locations.Add(loc);
                    }
                }



            }

            // King
            if ((pieceVal == (byte)0b00000001))
            {
                var possibleNextLocs = new List<int>();
                possibleNextLocs.Add((row + 1) * 8 + col);
                possibleNextLocs.Add((row - 1) * 8 + col);
                possibleNextLocs.Add((row - 1) * 8 + col - 1);
                possibleNextLocs.Add((row + 1) * 8 + col - 1);
                possibleNextLocs.Add((row + 1) * 8 + col + 1);
                possibleNextLocs.Add((row - 1) * 8 + col + 1);
                possibleNextLocs.Add((row ) * 8 + col + 1);
                possibleNextLocs.Add((row ) * 8 + col - 1);
                foreach (int loc in possibleNextLocs)
                {
                    if (this.checkPossibleMoveToLocation(loc, pieceColor, newBoardToCheck))
                    {
                        locations.Add(loc);
                    }
                }

            }

            return locations;
        }

        private bool isCheckmate(byte[] newBoard, int newcolorTurn)
        {
            byte[] tempBoard = new byte[64];
            if(isCheck(newBoard, newcolorTurn))
            {
                int boardLocation = 0;
                foreach(int piece in newBoard)
                {
                    Array.Copy(newBoard, tempBoard, 64);
                    //check moving all my pieces still remains check
                    if (piece >> 6 == newcolorTurn)
                    {
                        var possibleMoves = this.GetPossibleMoveLocations(piece, boardLocation, newBoard);
                        possibleMoves = CleanPossibleLocation(possibleMoves, (byte)piece, boardLocation, tempBoard);
                        if (possibleMoves.Count > 0)
                        {
                            return false;
                        }

                        //foreach (int move in possibleMoveLocations)
                        //{
                        //    Array.Copy(newBoard, tempBoard, 64);
                        //    tempBoard[boardLocation] = 0;
                        //    tempBoard[move] = 
                        //    possibleMoves = CleanPossibleLocation(possibleMoveLocations, (byte)piece, move, tempBoard);
                        //    if (possibleMoveLocations.Count > 0)
                        //    {
                        //        return false;
                        //    }

                        //}
                    }

                boardLocation++;
                }
                return true;
            }
            return false;
        }

        private bool isCheck(byte[] newBoard, int newcolorTurn)
        {
            //check possible moves of all of the oponents pieces and see if the kings location is in any of them.
            var kingVal = newcolorTurn == 0 ? 1 : 0b01000001;
            int kingLocation = Array.FindIndex(newBoard, x => x == (byte)kingVal);
            int boardLocation = 0;
            foreach(int piece in newBoard)
            {
                //if piece is not current player
                if(piece >> 6 != newcolorTurn)
                {
                    var possibleLocations = this.GetPossibleMoveLocations(piece, boardLocation, newBoard);
                    if (possibleLocations.Contains(kingLocation))
                    {
                        return true;
                    }

                }
                boardLocation++;
            }

            return false;

        }

        private void TranslateFen(string fen, byte[] board)
        {
            Array.Clear(board, (byte)0, board.Length);
            var fenVals = fen.ToCharArray();
            var boardLocation = 0;
            foreach (Char entry in fenVals)
            {
                byte boardVal = (byte)(Char.IsLower(entry) ? 0b01000000 : 0);
                switch (Char.ToUpper(entry))
                {
                    case 'K':
                        boardVal += (byte)0b00000001;
                        break;
                    case 'Q':
                        boardVal += (byte)0b00000010;
                        break;
                    case 'B':
                        boardVal += (byte)0b00000011;
                        break;
                    case 'N':
                        boardVal += (byte)0b00000100;
                        break;
                    case 'R':
                        boardVal += (byte)0b00000101;
                        break;
                    case 'P':
                        boardVal += (byte)0b00000110;
                        break;
                    default:
                        var entryChar = entry;
                        if (char.IsDigit(entryChar))
                        {
                            boardLocation += (int)Char.GetNumericValue(entry);
                        }
                        continue;
                }
                board[boardLocation] = boardVal;
                boardLocation++;

            }
        }

    }
}
