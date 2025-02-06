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
                        this.possibleMoveLocations = this.GetPossibleMoveLocations(this.slectedPiece.value, this.slectedPiece.currentBoardLocation);
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

        private void CompletMove( int boardLoc)
        {
            if (boardLoc >= 0 && boardLoc < 64 && this.possibleMoveLocations.Contains(boardLoc))
            {
                if (isCheck())
                {
                    return;
                }

                //if (boardLoc >= 0 && boardLoc < 64 )
                board[this.slectedPiece.currentBoardLocation] = 0b0;
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
            string output = isCheck() ? "Check" : "Not Check";

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

        private void addToListOfPossibleLocationPawn(int loc, ref List<int> locations)
        {
            if (this.board[loc] == 0)
            {
                locations.Add(loc);
            }
        }

        private void addRookMovement(int row, int col, int pieceColor, int pieceLocation, ref List<int> locations)
        {
            var newLoc = pieceLocation;

            for (int i = row + 1; i < 8; i++)
            {
                newLoc = i * 8 + col;
                if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(newLoc, ref locations);
                if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 != pieceColor)
                {
                    locations.Add(newLoc);
                    break;
                }

            }
            
            for(int i = row - 1; i >= 0; i--)
            {
                newLoc = i * 8 + col;
                if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(newLoc, ref locations);
                if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 != pieceColor)
                {
                    locations.Add(newLoc);
                    break;
                }

            }

            for (int i = col + 1; i < 8; i++)
            {
                newLoc = row * 8 + i;
                if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(newLoc, ref locations);
                if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 != pieceColor)
                {
                    locations.Add(newLoc);
                    break;
                }

            }

            for (int i = col - 1; i >= 0; i--)
            {
                newLoc = row * 8 + i;
                if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(newLoc, ref locations);
                if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 != pieceColor)
                {
                    locations.Add(newLoc);
                    break;
                }

            }

        }

        private void addBishopMovement(int row, int col, int pieceColor, int pieceLocation, ref List<int> locations)
        {
            int nextLoc = pieceLocation;

            //first Diag positive side
            for (int i = col + 1; i < 8; i++)
            {
                nextLoc = (i) + (row - (col - i)) * 8;
                if (nextLoc > 63 || nextLoc < 0)
                    break;
                if (this.board[nextLoc] != 0 && this.board[nextLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(nextLoc, ref locations);
                if (this.board[nextLoc] != 0 && this.board[nextLoc] >> 6 != pieceColor)
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
                if (this.board[nextLoc] != 0 && this.board[nextLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(nextLoc, ref locations);
                if (this.board[nextLoc] != 0 && this.board[nextLoc] >> 6 != pieceColor)
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
                if (this.board[nextLoc] != 0 && this.board[nextLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(nextLoc, ref locations);
                if (this.board[nextLoc] != 0 && this.board[nextLoc] >> 6 != pieceColor)
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
                if (this.board[nextLoc] != 0 && this.board[nextLoc] >> 6 == pieceColor)
                    break;
                this.addToListOfPossibleLocationPawn(nextLoc, ref locations);
                if (this.board[nextLoc] != 0 && this.board[nextLoc] >> 6 != pieceColor)
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

        private bool checkPossibleMoveToLocation(int boardLoc,int pieceColor)
        {
            if (boardLoc > 63 || boardLoc < 0)
               return false;
            if (this.board[boardLoc] != 0 && this.board[boardLoc] >> 6 == pieceColor)
                return false;
            return true;
        }

        private List<int> GetPossibleMoveLocations(int piece, int pieceLocation)
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
                    this.addToListOfPossibleLocationPawn(pieceLocation+8, ref locations);
                    if(locations.Count > 0)
                    this.addToListOfPossibleLocationPawn(pieceLocation+16, ref locations);
                }
                else if (pieceLocation / 8 == 6 && pieceColor == 0)
                {
                    this.addToListOfPossibleLocationPawn(pieceLocation - 8, ref locations);
                    if (locations.Count > 0)
                    this.addToListOfPossibleLocationPawn(pieceLocation - 16, ref locations);
                }
                else
                {
                    var newLoc = pieceColor == 1 ? pieceLocation + 8 : pieceLocation - 8;
                    this.addToListOfPossibleLocationPawn(newLoc, ref locations);
                    var captureLoc1 = pieceColor == 1 ? pieceLocation + 9 : pieceLocation - 9;
                    var captureLoc2 = pieceColor == 1 ? pieceLocation + 7 : pieceLocation - 7;
                    if (this.board[captureLoc1] != 0  && captureLoc1 /8 != pieceLocation)
                    {
                        if (board[captureLoc1] >> 6 != pieceColor)
                            locations.Add(captureLoc1);
                    }
                    if (this.board[captureLoc2] != 0 && captureLoc2 / 8 != pieceLocation / 8)
                    {
                        if (board[captureLoc1] >> 6 != pieceColor)
                            locations.Add(captureLoc2);
                    }
                }
            }

            // rook
            if(pieceVal == (byte)0b00000101)
            {
                this.addRookMovement(row, col, pieceColor, pieceLocation, ref locations);
            }
            
            // bishop
            if(pieceVal == (byte)0b00000011)
            {
               
                this.addBishopMovement(row, col, pieceColor, pieceLocation, ref locations);              
            }

            // queen
            if (pieceVal == (byte)0b00000010)
            {

                this.addBishopMovement(row, col, pieceColor, pieceLocation, ref locations);
                this.addRookMovement(row, col, pieceColor, pieceLocation, ref locations);
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
                    if(this.checkPossibleMoveToLocation(loc, pieceColor))
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
                    if (this.checkPossibleMoveToLocation(loc, pieceColor))
                    {
                        locations.Add(loc);
                    }
                }

            }

            return locations;
        }

        private bool isCheck()
        {
            //check possible moves of all of the oponents pieces and see if the kings location is in any of them.
            var kingVal = colorTurn == 0 ? 1 : 0b01000001;
            int kingLocation = Array.FindIndex(board, x => x == (byte)kingVal);
            int boardLocation = 0;
            foreach(int piece in board)
            {
                if(piece >> 6 != colorTurn)
                {
                    var possibleLocations = this.GetPossibleMoveLocations(piece, boardLocation);
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
