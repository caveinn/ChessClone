using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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

        private Piece draggedPiece;

        private Dictionary<string, Texture2D> pieceTextures = new(){};

        private Byte[] board = new byte[64];

        private string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";




        public Texture2D Texture;

        private enum Side { white, black }

        private class Piece {
            public int value { get; set; }
            public int currentBoardLocation { get; set; }
        }

       

        //private class Piece
        //{
        //    public int X { get; set; }
        //    public int Y { get; set; }

        //    public Side side { get; set; }

        //    public Texture2D texture { get; set; }

        //    public string name { get; set; }

        //    public bool isSelected { get; set; }

        //    public bool isDragged { get; set; }

        //    public int boardX { get; set; }
        //    public int boardY { get; set; }

        //    public void drawPiece(SpriteBatch _spriteBatch, int tileWidth)
        //    {
        //        _spriteBatch.Draw(this.texture, new Rectangle(this.X, this.Y , tileWidth, tileWidth), this.isSelected ? Color.Red : Color.White);
        //    }
        //}



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

            // TODO: use this.Content to load your game content here
            //ballTexture = Content.Load<Texture2D>("ball");
            foreach (string piece in this.pieces)
            {
                this.pieceTextures.Add($"white-{piece}", Content.Load<Texture2D>($"white-{piece}"));
                this.pieceTextures.Add($"black-{piece}", Content.Load<Texture2D>($"black-{piece}"));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            //int tileWidth = (GameBounds.X - Padding * 2) / 8;
            // TODO: Add your update logic here
            //var mouseState = Mouse.GetState();
            //if(mouseState.LeftButton == ButtonState.Pressed)
            //{
            //    foreach (Piece piece in allPieces)
            //    {
            //        Console.WriteLine(piece.name);
            //        var rect = new Rectangle(piece.X , piece.Y , tileWidth, tileWidth);
            //        if (rect.Contains(mouseState.X, mouseState.Y) && !this.PieceIsSelected)
            //        {
            //            piece.isSelected = true;
            //            this.PieceIsSelected = true;
            //        }
            //        if (piece.isSelected)
            //        {
            //            piece.X = mouseState.X - tileWidth/2;
            //            piece.Y = mouseState.Y - tileWidth/2;
            //        }
            //    }
            //}

            //if (mouseState.LeftButton == ButtonState.Released)
            //{
            //    foreach (Piece piece in allPieces)
            //    {
            //        if (this.PieceIsSelected && piece.isSelected)
            //        {
            //            piece.isSelected = false;
            //            this.PieceIsSelected = false;
            //            double xLocation = (double)(mouseState.X - Padding) / (double)tileWidth;
            //            double yLocation = (double)(mouseState.Y - Padding) / (double)tileWidth;
            //            piece.X =(int)Math.Floor(xLocation) * tileWidth + Padding;
            //            piece.Y = (int)Math.Floor(yLocation) * tileWidth + Padding;
            //        }

            //    }
            //}
            MouseState msState = Mouse.GetState();

            if(msState.LeftButton == ButtonState.Pressed)
            {
                if (!this.isDraging)
                {
                    var boardLoc = (msState.Y - Padding) / tileWidth * 8 + (msState.X - Padding) / tileWidth;
                    var selectePice = boardLoc >= 0 && boardLoc < 64 ? board[boardLoc] : 0;
                    if (selectePice != 0)
                    {
                        this.draggedPiece = new Piece();
                        this.draggedPiece.currentBoardLocation = boardLoc;
                        this.draggedPiece.value = selectePice;
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
                        board[this.draggedPiece.currentBoardLocation] = 0b0;
                        board[boardLoc] = (byte)this.draggedPiece.value;
                        
                    }
                    //var selectePice = boardLoc >= 0 && boardLoc < 64 ? board[boardLoc] : 0;
                    //if (selectePice != 0)
                    //{
                    //    this.draggedPiece = new Piece();
                    //    this.draggedPiece.currentBoardLocation = boardLoc;
                    //    this.draggedPiece.value = selectePice;
                    //    this.isDraging = true;
                    //}
                    this.isDraging = false;
                    this.draggedPiece = null;
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            //int ytileCount = 0;
            //int xtileCount = 0;
            //for(int i = 0; i < 8; i++)
            //{
            //    _spriteBatch.DrawString(SpriteFont.,);
            //}
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
                    if (this.isDraging && this.draggedPiece.currentBoardLocation == boardInt)
                    {
                        _spriteBatch.Draw(texture, new Rectangle(Mouse.GetState().X - tileWidth/2, Mouse.GetState().Y - tileWidth / 2, tileWidth, tileWidth), Color.Red);
                    }
                    else
                    {
                        _spriteBatch.Draw(texture, new Rectangle(Padding + (j * tileWidth), Padding + (i * tileWidth), tileWidth, tileWidth), Color.White);
                    }
                    
                }
            }

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
                        _spriteBatch.Draw(texture, new Rectangle(Padding + (j * tileWidth), Padding + (i * tileWidth), tileWidth, tileWidth), Color.White);
                       
                    }
                    else if(this.draggedPiece.currentBoardLocation == boardInt)
                    {
                        _spriteBatch.Draw(texture, new Rectangle(Mouse.GetState().X - tileWidth / 2, Mouse.GetState().Y - tileWidth / 2, tileWidth, tileWidth), Color.Red);
                    }
                }
            }
                    //foreach(Piece piece in this.allPieces)
                    //{
                    //    //_spriteBatch.Draw(piece.texture, new Rectangle(Padding + piece.X * tileWidth, Padding + piece.Y * tileWidth, tileWidth, tileWidth), piece.isSelected ? Color.Red : Color.White);
                    //    piece.drawPiece(_spriteBatch, tileWidth);
                    //}

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

        private void DrawTile(SpriteBatch _spriteBatch, byte boardVal,int X,int Y)
        {
          
        }

    }
}
