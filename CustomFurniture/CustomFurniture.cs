﻿using System;
using System.IO;

using StardewValley;
using StardewValley.Objects;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using CustomElementHandler;
using System.Collections.Generic;

namespace CustomFurniture
{
    class CustomFurniture : Furniture, ISaveElement
    {
        private Texture2D texture;
        private int animationFrames;
        private int frame;
        private Rectangle animatedSourceRect;
        private int frameWidth;
        private int skipFrame;
        private int counter;
        public CustomFurnitureData data;
        public string id;

        public CustomFurniture()
        {

        }

        public CustomFurniture(CustomFurnitureData data, string objectID, Vector2 tile)
        {
            build(data, objectID, tile);
        }

        public void build(CustomFurnitureData data, string objectID, Vector2 tile)
        {
            id = objectID;
            texture = CustomFurnitureMod.helper.Content.Load<Texture2D>(Path.Combine("Furniture", data.folderName, data.texture));
            animationFrames = data.animationFrames;
            this.data = data;
            frameWidth = data.setWidth;
            frame = 0;
            skipFrame = 60 / data.fps;
            counter = 0;
            tileLocation = tile;

            CustomFurnitureMod.helper.Reflection.GetPrivateField<string>(this, "_description").SetValue(data.description);

            parentSheetIndex = data.index;

            name = data.name;
            furniture_type = getTypeFromName(data.type);
            defaultSourceRect = new Rectangle(data.index * 16 % texture.Width, data.index * 16 / texture.Width * 16, 1, 1);
            drawHeldObjectLow = false;

            defaultSourceRect.Width = data.width;
            defaultSourceRect.Height = data.height;
            sourceRect = new Rectangle(data.index * 16 % texture.Width, data.index * 16 / texture.Width * 16, defaultSourceRect.Width * 16, defaultSourceRect.Height * 16);
            animatedSourceRect = sourceRect;
            defaultSourceRect = sourceRect;

            defaultBoundingBox = new Rectangle((int)tileLocation.X, (int)tileLocation.Y, 1, 1);


            defaultBoundingBox.Width = data.boxWidth;
            defaultBoundingBox.Height = data.boxHeight;
            boundingBox = new Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize, defaultBoundingBox.Width * Game1.tileSize, defaultBoundingBox.Height * Game1.tileSize);
            defaultBoundingBox = boundingBox;

            updateDrawPosition();
            rotations = data.rotations;
            price = data.price;

        }

        protected override string loadDisplayName()
        {
            return name;
        }

        public override string getDescription()
        {
            string text = description;
            SpriteFont smallFont = Game1.smallFont;
            int width = Game1.tileSize * 4 + Game1.tileSize / 4;
            return Game1.parseText(text, smallFont, width);
        }

        private int getTypeFromName(String type)
        {
            switch (type)
            {
                case "chair": return chair;
                case "bench": return bench;
                case "couch": return couch;
                case "armchair": return armchair;
                case "dresser": return dresser;
                case "long table": return longTable;
                case "painting": return painting;
                case "lamp": return lamp;
                case "decor": return decor;
                case "bookcase": return bookcase;
                case "table": return table;
                case "rug": return rug;
                case "window": return window;
                default: return other;
            }
        }

        public override Item getOne()
        {
            return this;
        }

        private float getScaleSize()
        {
            int num1 = sourceRect.Width / 16;
            int num2 = sourceRect.Height / 16;
            if (num1 >= 5)
                return 0.75f;
            if (num2 >= 3)
                return 1f;
            if (num1 <= 2)
                return 2f;
            return num1 <= 4 ? 1f : 0.1f;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            spriteBatch.Draw(texture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Rectangle?(defaultSourceRect), Color.White * transparency, 0.0f, new Vector2((float)(defaultSourceRect.Width / 2), (float)(defaultSourceRect.Height / 2)), 1f * getScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            counter++;
            counter = counter > skipFrame ? 0 : counter;
            frame = counter == 0 ? frame + 1 : frame;
            frame = frame >= animationFrames ? 0 : frame;
            int offset = frameWidth * 16 * frame;
            animatedSourceRect = frame == 0 ? sourceRect :  new Rectangle(offset + sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);

            if (x == -1)
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, drawPosition), new Rectangle?(animatedSourceRect), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, furniture_type == 12 ? 0.0f : (float)(boundingBox.Bottom - 8) / 10000f);
            else
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize - (sourceRect.Height * Game1.pixelZoom - boundingBox.Height)))), new Rectangle?(sourceRect), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.furniture_type == 12 ? 0.0f : (float)(boundingBox.Bottom - 8) / 10000f);
            if (heldObject == null)
                return;
            if (heldObject is Furniture ho)
            {
                customDrawAtNonTileSpot(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(boundingBox.Center.X - Game1.tileSize / 2), (float)(boundingBox.Center.Y - ho.sourceRect.Height * Game1.pixelZoom - (drawHeldObjectLow ? -Game1.tileSize / 4 : Game1.tileSize / 4)))), (float)(boundingBox.Bottom - 7) / 10000f, alpha);
            }
            else
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(boundingBox.Center.X - Game1.tileSize / 2), (float)(boundingBox.Center.Y - (drawHeldObjectLow ? Game1.tileSize / 2 : Game1.tileSize * 4 / 3)))) + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize * 5 / 6)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f);
                spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(boundingBox.Center.X - Game1.tileSize / 2), (float)(boundingBox.Center.Y - (drawHeldObjectLow ? Game1.tileSize / 2 : Game1.tileSize * 4 / 3)))), new Rectangle?(Game1.currentLocation.getSourceRectForObject(heldObject.ParentSheetIndex)), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, (float)(boundingBox.Bottom + 1) / 10000f);
            }
        }

        private void customDrawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
            spriteBatch.Draw(texture, location, new Rectangle?(sourceRect), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        public object getReplacement()
        {
            return new Furniture(0, tileLocation, currentRotation);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> savedata = new Dictionary<string, string>();
            savedata.Add("name", name);
            savedata.Add("id", id);
            savedata.Add("rotation", currentRotation.ToString());
            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            if (replacement is Furniture f)
            {
                string id = additionalSaveData["id"];
                build(CustomFurnitureMod.furniture[id].data, id, f.tileLocation);
                int targetRotation = additionalSaveData.ContainsKey("rotation") ? int.Parse(additionalSaveData["rotation"]) : f.currentRotation;
                while (currentRotation != targetRotation)
                {
                    rotate();
                }

            }
            
        }
    }
}
