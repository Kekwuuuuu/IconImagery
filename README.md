# IconImagery

A Unity Editor addon that converts any PNG/JPG image into a **circular icon** with transparent corners/background. Saves the result to `Assets/CustomNamePlate/Icons` at a configurable resolution (default 512×512).

## ✨ Features

- **One‑click circle crop** – load any square or rectangular image, get a perfect circle.
- **Transparent background** – everything outside the circle becomes fully transparent.
- **Preserves original colours** – the circle area keeps the source image exactly as it is.
- **Adjustable output size** – choose any resolution between 32×32 and 2048×2048 (default 512).
- **Auto‑saving** – saves directly to `Assets/CustomNamePlate/Icons`, no file dialog.
- **Smart naming** – filename based on the original asset name (e.g., `MyLogo.png` → `MyLogo.png`).
- **Duplicate handling** – appends `_1`, `_2`, etc. if a file already exists.
- **Sprite import** – output PNG is automatically imported as a Sprite.

## 🚀 Usage

1. Open the window: **Window → IconImagery**.
2. Drag a **Texture2D** (PNG or JPG) into the `Icon Photo` slot.  
   *You can drag from the Project view or assign via the object picker.*
3. (Optional) Adjust the `Output Size (px)` – the result will be a square of that size.
4. Click **Crop to Circle & Save**.
5. The circular PNG appears in `Assets/CustomNamePlate/Icons/YourIconName.png`.

## 🧪 Example

Any shape (square or rectangle) is scaled to fill the circle; outer parts become transparent.

## 🛠 Requirements

- Unity 2019.4 or higher.
- Input texture must be readable (most imported PNG/JPG are readable by default).

## 💡 Tips

- For best results, start with a **square source image** – the tool will stretch it to fill the circle.
- If your source has its own transparent background, that transparency is preserved **inside** the circle (useful for logos with holes).
- Use 512×512 output for VRChat nameplate decals (works perfectly with the `CustomNamePlates` shader).
- Combine with **StringImagery** to generate both name text and round icons for nameplates.

## 📄 License

MIT – free to use, modify, and distribute.
