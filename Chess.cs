using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chess
{
    public class Chess : Game
    {
        //Texture2D ballTexture;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Point GameBounds = new Point(720, 720); // Game resolutions
        private int tileWidth;
        const int Padding = 20;
        private bool PieceIsSelected = false;
        private string[] pieces = [ "king", "queen", "bishop", "knight", "rook", "pawn"];
        private List<string> drawnPieces = new List<string>();

        private bool isDraging = false;

        private Piece slectedPiece;

        private Dictionary<string, Texture2D> pieceTextures = new(){};

        private Byte[] board = new byte[64];

        private Texture2D whitedot;

        private string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        private bool whiteTurn = true;




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

            if(msState.LeftButton == ButtonState.Pressed)
            {
                if (!this.isDraging)
                {
                    var boardLoc = (msState.Y - Padding) / tileWidth * 8 + (msState.X - Padding) / tileWidth;
                    var selectePice = boardLoc >= 0 && boardLoc < 64 ? board[boardLoc] : 0;
                    if (selectePice != 0)
                    {
                        this.slectedPiece = new Piece();
                        this.slectedPiece.currentBoardLocation = boardLoc;
                        this.slectedPiece.value = selectePice;
                        this.isDraging = true;
                    }
                }
            }

            if (msState.LeftButton == ButtonState.Released)
            {
                if (this.isDraging)
                {
                    var boardLoc = (msState.Y - Padding) / tileWidth * 8 + (msState.X - Padding) / tileWidth;
                    if (boardLoc >= 0 && boardLoc < 64) {
                        board[this.slectedPiece.currentBoardLocation] = 0b0;
                        board[boardLoc] = (byte)this.slectedPiece.value;
                        this.slectedPiece.currentBoardLocation = boardLoc;
                    }

                    this.isDraging = false;
                    //this.draggedPiece = null;
                }
            }
            base.Update(gameTime);
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
                    if(this.slectedPiece != null)
                    {
                        var locations = this.GetPossibleMoveLocations();
                        foreach(int location in locations)
                        {
                            this.DrawPosibbleMoves(_spriteBatch, location);
                        }
                        
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

        private void DrawPosibbleMoves(SpriteBatch sb, int location)
        {
            int Y = location / 8;
            int X = location % 8;
            sb.Draw(this.whitedot, new Rectangle(Padding + (X * tileWidth) + tileWidth / 4, Padding + (Y * tileWidth)+tileWidth/4, tileWidth/2, tileWidth/2), Color.Gray * 0.03f);
        }

        private void addToListOfPossibleLocationPawn(int loc, ref List<int> locations)
        {
            if (this.board[loc] == 0)
            {
                locations.Add(loc);
            }
        }

        private List<int> GetPossibleMoveLocations()
        {
            var locations = new List<int>();
            var pieceVal = this.slectedPiece.value & 0b00001111;
            var pieceLocation = this.slectedPiece.currentBoardLocation;
            var pieceColor = this.slectedPiece.value >> 6;

            //pawn
            if (pieceVal == (byte)0b0110 && pieceLocation/8 != 0 && pieceLocation / 8 != 7)
            {
                if(pieceLocation/8 == 1)
                {
                    this.addToListOfPossibleLocationPawn(pieceLocation+8, ref locations);
                    if(locations.Count > 0)
                    this.addToListOfPossibleLocationPawn(pieceLocation+16, ref locations);
                }
                else if (pieceLocation / 8 == 6)
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
                var row = pieceLocation / 8;
                var col = pieceLocation % 8;

                for (int i = 1; i < 8 - row; i++ )
                {
                    var newLoc = pieceLocation + i * 8;
                    if(this.board[newLoc] != 0 && this.board[newLoc] >> 6 == pieceColor)
                        break;
                    this.addToListOfPossibleLocationPawn(newLoc, ref locations);
                    if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 != pieceColor)
                    {
                        locations.Add(newLoc);
                        break;
                    };
                }
                for (int i = 1; i < row; i++)
                {
                    var newLoc = pieceLocation - i * 8;
                    if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 == pieceColor)
                        break;
                    this.addToListOfPossibleLocationPawn(newLoc, ref locations);
                    if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 != pieceColor)
                    {
                        locations.Add(newLoc);
                        break;
                    }
                }
                for (int i = col; i > 0; i--)
                {
                    var newLoc = pieceLocation - i ;
                    if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 == pieceColor)
                        break;
                    this.addToListOfPossibleLocationPawn(newLoc, ref locations);
                    if (this.board[newLoc] != 0 && this.board[newLoc] >> 6 != pieceColor)
                    {
                        locations.Add(newLoc);
                        break;
                    }
                }
                for (int i = 1; i < 8 - col; i++)
                {
                    var newLoc = pieceLocation + i;
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

            return locations;
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
