using UnityEditor;
using UnityEngine;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// Draws the placeholder art in code and saves it as PNG files under Art/Placeholder.
    ///
    /// Each file is named after an item in the design.md section 8 content list, so it can be
    /// replaced one for one: overwrite the PNG with your own drawing of the same size and every
    /// tile, prefab and scene that uses it updates automatically. Existing files are never
    /// overwritten, so replaced art is safe when the builder runs again.
    ///
    /// The style follows the art rules: 32x32 grid, light from the top left, dark coloured
    /// outlines rather than pure black.
    /// </summary>
    public static class PlaceholderArt
    {
        const string Root = "Assets/_Project/Art/Placeholder/";

        // ----- Ground and wall tiles (32 x 32, centre pivot)
        public const string GrassLong = Root + "Tiles/ground_grass_long.png";
        public const string GrassCut = Root + "Tiles/ground_grass_cut.png";
        public const string PatioClean = Root + "Tiles/ground_patio_clean.png";
        public const string PatioWeedy = Root + "Tiles/ground_patio_weedy.png";
        public const string Dirt = Root + "Tiles/ground_dirt.png";
        public const string Path = Root + "Tiles/ground_path.png";
        public const string FenceIntact = Root + "Tiles/fence_intact.png";
        public const string FenceBroken = Root + "Tiles/fence_broken.png";
        public const string HouseWall = Root + "Tiles/wall_house.png";

        // ----- UI frames (9-sliced)
        public const string UiPanelDark = Root + "UI/ui_panel_dark.png";
        public const string UiPanelPaper = Root + "UI/ui_panel_paper.png";

        // A small warm palette shared by all placeholders.
        static readonly Color32 Outline = PixelCanvas.Hex("#2b1d14");
        static readonly Color32 GrassDark = PixelCanvas.Hex("#3d6e2d");
        static readonly Color32 GrassMid = PixelCanvas.Hex("#4f8a3a");
        static readonly Color32 GrassLight = PixelCanvas.Hex("#6aa84a");
        static readonly Color32 GrassPale = PixelCanvas.Hex("#86bf5c");
        static readonly Color32 SlabMid = PixelCanvas.Hex("#c9b79c");
        static readonly Color32 SlabLight = PixelCanvas.Hex("#ddd0b8");
        static readonly Color32 SlabDark = PixelCanvas.Hex("#a8957a");
        static readonly Color32 Grout = PixelCanvas.Hex("#8f7d66");
        static readonly Color32 DirtMid = PixelCanvas.Hex("#8a6440");
        static readonly Color32 DirtDark = PixelCanvas.Hex("#6e4e31");
        static readonly Color32 DirtLight = PixelCanvas.Hex("#a07a52");
        static readonly Color32 GravelMid = PixelCanvas.Hex("#b5a58c");
        static readonly Color32 GravelDark = PixelCanvas.Hex("#9a8a72");
        static readonly Color32 GravelLight = PixelCanvas.Hex("#d0c4ae");
        static readonly Color32 WoodMid = PixelCanvas.Hex("#9c6b3e");
        static readonly Color32 WoodLight = PixelCanvas.Hex("#b8854f");
        static readonly Color32 WoodDark = PixelCanvas.Hex("#5a3a22");
        static readonly Color32 SidingMid = PixelCanvas.Hex("#d8cfc0");
        static readonly Color32 SidingShadow = PixelCanvas.Hex("#b0a693");
        static readonly Color32 SidingLight = PixelCanvas.Hex("#ebe4d8");
        static readonly Color32 Paper = PixelCanvas.Hex("#f2ead8");
        static readonly Color32 PanelDark = PixelCanvas.Hex("#3a2a20");
        static readonly Color32 PanelEdge = PixelCanvas.Hex("#6e5440");

        /// <summary>Writes every missing placeholder file.</summary>
        public static void GenerateAll()
        {
            Tile(GrassLong, DrawGrassLong);
            Tile(GrassCut, DrawGrassCut);
            Tile(PatioClean, () => DrawPatio(false));
            Tile(PatioWeedy, () => DrawPatio(true));
            Tile(Dirt, DrawDirt);
            Tile(Path, DrawPath);
            Tile(FenceIntact, () => DrawFence(false));
            Tile(FenceBroken, () => DrawFence(true));
            Tile(HouseWall, DrawHouseWall);

            Frame(UiPanelDark, PanelDark, PanelEdge);
            Frame(UiPanelPaper, Paper, Grout);
        }

        static void Tile(string path, System.Func<PixelCanvas> draw)
        {
            if (PlaceholderAssetWriter.SavePngIfMissing(path, draw()))
                PlaceholderAssetWriter.ConfigureSprite(path, SpriteAlignment.Center);
        }

        /// <summary>A 12x12 UI frame with a 4 pixel border, stretched with 9-slicing.</summary>
        static void Frame(string path, Color32 fill, Color32 edge)
        {
            var c = new PixelCanvas(12, 12);
            c.FillRect(1, 1, 10, 10, fill);
            c.Outline(1, 1, 10, 10, edge);
            c.Outline(0, 0, 12, 12, Outline);
            // Clip the corners for a softer, rounded look.
            c.Set(0, 0, default); c.Set(11, 0, default); c.Set(0, 11, default); c.Set(11, 11, default);
            if (PlaceholderAssetWriter.SavePngIfMissing(path, c))
                PlaceholderAssetWriter.ConfigureSprite(path, SpriteAlignment.Center, new Vector4(4, 4, 4, 4));
        }

        // A repeatable "random" number from a position, so the art is the same every time it is drawn.
        static int Hash(int x, int y, int seed) => (int)(PlaceholderHash.Mix(x, y, seed) & 0x7fffffff);

        static PixelCanvas DrawGrassLong()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(GrassMid);
            // Tufts: short vertical blades, darker at the base and lighter at the tip.
            for (int i = 0; i < 26; i++)
            {
                int x = Hash(i, 1, 7) % 32, y = Hash(i, 2, 7) % 29;
                c.Set(x, y, GrassDark);
                c.Set(x, y + 1, GrassMid);
                c.Set(x, y + 2, GrassLight);
                if (i % 3 == 0) c.Set(x + 1, y + 3, GrassPale);
            }
            return c;
        }

        static PixelCanvas DrawGrassCut()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(GrassLight);
            // Mower stripes and a few specks.
            for (int y = 0; y < 32; y++)
                if ((y / 8) % 2 == 0)
                    for (int x = 0; x < 32; x++) c.Set(x, y, GrassMid);
            for (int i = 0; i < 12; i++) c.Set(Hash(i, 3, 5) % 32, Hash(i, 4, 5) % 32, GrassPale);
            return c;
        }

        static PixelCanvas DrawPatio(bool weedy)
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(Grout);
            // Four 15x15 slabs with a one-pixel grout line, lit from the top left.
            for (int sy = 0; sy < 2; sy++)
                for (int sx = 0; sx < 2; sx++)
                {
                    int x0 = sx * 16, y0 = sy * 16;
                    c.FillRect(x0, y0 + 1, 15, 15, SlabMid);
                    for (int i = 0; i < 15; i++) { c.Set(x0 + i, y0 + 15, SlabLight); c.Set(x0, y0 + 1 + i, SlabLight); }
                    for (int i = 1; i < 15; i++) { c.Set(x0 + i, y0 + 1, SlabDark); c.Set(x0 + 14, y0 + i, SlabDark); }
                }

            if (weedy)
            {
                // Weeds pushing up through the grout lines.
                int[,] weeds = { { 15, 4 }, { 15, 22 }, { 6, 15 }, { 25, 15 }, { 15, 15 } };
                for (int w = 0; w < weeds.GetLength(0); w++)
                {
                    int x = weeds[w, 0], y = weeds[w, 1];
                    c.Set(x, y, GrassDark); c.Set(x, y + 1, GrassMid); c.Set(x - 1, y + 2, GrassLight);
                    c.Set(x + 1, y + 2, GrassLight); c.Set(x + 1, y + 1, GrassMid);
                }
            }
            return c;
        }

        static PixelCanvas DrawDirt()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(DirtMid);
            for (int i = 0; i < 30; i++) c.Set(Hash(i, 5, 3) % 32, Hash(i, 6, 3) % 32, i % 2 == 0 ? DirtDark : DirtLight);
            return c;
        }

        static PixelCanvas DrawPath()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(GravelMid);
            // Small stones: a light top-left pixel and a dark bottom-right pixel.
            for (int i = 0; i < 22; i++)
            {
                int x = Hash(i, 7, 9) % 31, y = 1 + Hash(i, 8, 9) % 31;
                c.Set(x, y, GravelLight);
                c.Set(x + 1, y - 1, GravelDark);
            }
            return c;
        }

        static PixelCanvas DrawFence(bool broken)
        {
            // Transparent background, so the ground tile shows between the pickets.
            var c = new PixelCanvas(32, 32);

            // Two horizontal rails behind the pickets.
            c.FillRect(0, 9, 32, 3, WoodDark);
            c.FillRect(0, 21, 32, 3, WoodDark);

            // Four pickets, 6 pixels wide with a 2 pixel gap, and pointed tops.
            for (int p = 0; p < 4; p++)
            {
                if (broken && (p == 1 || p == 2)) continue; // missing pickets
                int x0 = p * 8 + 1;
                Picket(c, x0, 0, 28);
            }

            if (broken)
            {
                // A loose picket lying at the bottom of the gap.
                c.FillRect(9, 1, 13, 4, WoodMid);
                c.Outline(9, 1, 13, 4, WoodDark);
                for (int i = 10; i < 21; i++) c.Set(i, 3, WoodLight);
            }
            return c;
        }

        static void Picket(PixelCanvas c, int x0, int y0, int height)
        {
            c.FillRect(x0, y0, 6, height, WoodMid);
            for (int j = y0; j < y0 + height; j++) { c.Set(x0, j, WoodLight); c.Set(x0 + 5, j, WoodDark); }
            // Pointed top.
            c.FillRect(x0 + 1, y0 + height, 4, 1, WoodMid);
            c.FillRect(x0 + 2, y0 + height + 1, 2, 1, WoodLight);
            c.Outline(x0, y0, 6, height, WoodDark);
        }

        static PixelCanvas DrawHouseWall()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(SidingMid);
            // Horizontal clapboard siding: 8 pixel boards with a shadow under each overlap.
            for (int y = 0; y < 32; y += 8)
                for (int x = 0; x < 32; x++)
                {
                    c.Set(x, y, SidingShadow);
                    c.Set(x, y + 7, SidingLight);
                }
            return c;
        }
    }
}
