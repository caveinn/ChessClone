using System;
using System.Collections.Generic;
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
        

        public Texture2D Texture;

        private enum Side { white, black }
        private class Piece
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Side side { get; set; }

            public Texture2D texture { get; set; }

            public string name { get; set; }
            
            public bool isSelected { get; set; }

            public bool isDragged { get; set; }

            public int boardX { get; set; }
            public int boardY { get; set; }

            public void drawPiece(SpriteBatch _spriteBatch, int tileWidth)
            {
                _spriteBatch.Draw(this.texture, new Rectangle(this.X, this.Y , tileWidth, tileWidth), this.isSelected ? Color.Red : Color.White);
            }
        }

        private class Pawn : Piece { 
             public Pawn() { 
                this.name = "pawn";
            }  
        }

        private class King : Piece
        {
            public King()
            {
                this.name = "king";
            }
        }

        private class Queen : Piece
        {
            public Queen()
            {
                this.name = "queen";
            }
        }

        private class Bishop : Piece
        {
            public Bishop()
            {
                this.name = "bishop";
            }
        }

        private class Knight : Piece
        {
            public Knight()
            {
                this.name = "bishop";
            }
        }

        private class Rook : Piece
        {
            public Rook()
            {
                this.name = "rook";
            }
        }

        // set up black pieces
        List<Piece> allPieces = new List<Piece>();
 
        public Chess()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = GameBounds.X;
            _graphics.PreferredBackBufferHeight = GameBounds.Y;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            tileWidth = (GameBounds.X - Padding * 2) / 8;
        }

        private List<Piece> initilaizePieces(Side side)
        {
            int pieceRow = side == Side.white ? 0 : 7;
            int pawnRow = side == Side.white ? 1 : 6;
            int pieceRowLocation = Padding + pieceRow * tileWidth;
            Piece King = new King();
            King.X = Padding + 4 * tileWidth;
            King.boardX = 4;
            King.Y = pieceRowLocation;
            King.boardY = pieceRow;
            Piece Queen = new Queen();
            Queen.X = Padding + 3 * tileWidth;
            Queen.boardX = 3;
            Queen.Y = pieceRowLocation;
            Queen.boardY = pieceRow;
            Piece Bishop = new Bishop();
            Bishop.X = Padding + 2 * tileWidth;
            Bishop.boardX = 2;
            Bishop.Y = pieceRowLocation;
            Bishop.boardY = pieceRow;
            Piece Bishop2 = new Bishop();
            Bishop2.X = Padding + 5 * tileWidth;
            Bishop2.boardX = 5;
            Bishop2.Y = pieceRowLocation;
            Bishop2.boardY = pieceRow;
            Piece Knight = new Knight();
            Knight.X = Padding + 6 * tileWidth;
            Knight.boardX = 6;  
            Knight.Y = pieceRowLocation;
            Knight.boardY = pieceRow;
            Piece Knight2 = new Knight();
            Knight2.X = Padding + 1 * tileWidth;
            Knight2.boardX = 1;
            Knight2.Y = pieceRowLocation;
            Knight2.boardY = pieceRow;
            Piece Rook = new Rook();
            Rook.X = Padding + 7 * tileWidth;
            Rook.boardX = 7;
            Rook.Y = pieceRowLocation;
            Rook.boardY = pieceRow;
            Piece Rook2 = new Rook();
            Rook2.X = Padding  +  0;
            Rook2.boardX = 0;
            Rook2.Y = pieceRowLocation;
            Rook2.boardY = pieceRow;

            List<Piece> pieces = new List<Piece>
            {
                King, Queen, Bishop, Bishop2, Knight, Knight2, Rook, Rook2
            };
            for (int i=0; i < 8; i++)
            {
                Pawn pawn = new Pawn();
                pawn.Y = Padding + pawnRow * this.tileWidth;
                pawn.X = Padding + i * this.tileWidth;
                pieces.Add(pawn);
            }

            foreach(Piece piece in pieces)
            {
                piece.side = side;

            }

            return pieces;

        }


        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            if (Texture == null)
            {   //create texture to draw with if it does not exist
                Texture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
                Texture.SetData<Color>(new Color[] { Color.White });
            }

            this.allPieces = this.initilaizePieces(Side.white);
            allPieces.AddRange(this.initilaizePieces(Side.black));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //ballTexture = Content.Load<Texture2D>("ball");
            foreach(Piece piece in this.allPieces)
            {
                piece.texture = Content.Load<Texture2D>($"{piece.side.ToString()}-{piece.name}");
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            int tileWidth = (GameBounds.X - Padding * 2) / 8;
            // TODO: Add your update logic here
            var mouseState = Mouse.GetState();
            if(mouseState.LeftButton == ButtonState.Pressed)
            {
                foreach (Piece piece in allPieces)
                {
                    Console.WriteLine(piece.name);
                    var rect = new Rectangle(piece.X , piece.Y , tileWidth, tileWidth);
                    if (rect.Contains(mouseState.X, mouseState.Y) && !this.PieceIsSelected)
                    {
                        piece.isSelected = true;
                        this.PieceIsSelected = true;
                    }
                    if (piece.isSelected)
                    {
                        piece.X = mouseState.X - tileWidth/2;
                        piece.Y = mouseState.Y - tileWidth/2;
                    }
                }
            }

            if (mouseState.LeftButton == ButtonState.Released)
            {
                foreach (Piece piece in allPieces)
                {
                    if (this.PieceIsSelected && piece.isSelected)
                    {
                        piece.isSelected = false;
                        this.PieceIsSelected = false;
                        double xLocation = (double)(mouseState.X - Padding) / (double)tileWidth;
                        double yLocation = (double)(mouseState.Y - Padding) / (double)tileWidth;
                        piece.X =(int)Math.Floor(xLocation) * tileWidth + Padding;
                        piece.Y = (int)Math.Floor(yLocation) * tileWidth + Padding;
                    }

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
            int tileCount = 0;
            //for(int i = 0; i < 8; i++)
            //{
            //    _spriteBatch.DrawString(SpriteFont.,);
            //}
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    tileCount++;
                    this.DrawRectangle(_spriteBatch, new Rectangle(Padding + tileWidth*j, Padding + tileWidth * i, tileWidth , tileWidth), (i+j) % 2  == 0 ? Color.Wheat : Color.BurlyWood);

                };
            }
            foreach(Piece piece in this.allPieces)
            {
                //_spriteBatch.Draw(piece.texture, new Rectangle(Padding + piece.X * tileWidth, Padding + piece.Y * tileWidth, tileWidth, tileWidth), piece.isSelected ? Color.Red : Color.White);
                piece.drawPiece(_spriteBatch, tileWidth);
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

    }
}
