from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
TEMP = ROOT / "Temp" / "ImageGen" / "ItemSprites"
OUTPUT = ROOT / "Assets" / "Art" / "Sprites" / "Items"
CANVAS_SIZE = (1024, 1024)

SPRITES = {
    "icon_sickle.png": ("sickle_transparent.png", (620, 620)),
    "icon_wheat.png": ("wheat_transparent.png", (520, 620)),
    "icon_corn.png": ("corn_transparent.png", (520, 620)),
    "icon_chicken_feed.png": ("chicken_feed_transparent.png", (560, 620)),
    "icon_empty_basket.png": ("empty_basket_transparent.png", (650, 560)),
    "icon_bread.png": ("bread_transparent.png", (620, 500)),
}


def crop_subject(path: Path) -> Image.Image:
    image = Image.open(path).convert("RGBA")
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise ValueError(f"No visible pixels found in {path}")
    return image.crop(bounds)


def normalize(source: Path, max_size: tuple[int, int]) -> Image.Image:
    subject = crop_subject(source)
    scale = min(max_size[0] / subject.width, max_size[1] / subject.height)
    size = (
        max(1, round(subject.width * scale)),
        max(1, round(subject.height * scale)),
    )
    subject = subject.resize(size, Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", CANVAS_SIZE, (0, 0, 0, 0))
    position = (
        (CANVAS_SIZE[0] - size[0]) // 2,
        (CANVAS_SIZE[1] - size[1]) // 2,
    )
    canvas.alpha_composite(subject, position)
    return canvas


def validate(path: Path) -> tuple[int, int, int, int]:
    image = Image.open(path).convert("RGBA")
    if image.size != CANVAS_SIZE:
        raise ValueError(f"Unexpected size for {path}: {image.size}")

    alpha = image.getchannel("A")
    corners = (
        alpha.getpixel((0, 0)),
        alpha.getpixel((1023, 0)),
        alpha.getpixel((0, 1023)),
        alpha.getpixel((1023, 1023)),
    )
    if corners != (0, 0, 0, 0):
        raise ValueError(f"Non-transparent corner in {path}: {corners}")

    bounds = alpha.getbbox()
    if bounds is None:
        raise ValueError(f"No visible sprite in {path}")
    return bounds


def checkerboard(size: tuple[int, int], square: int = 24) -> Image.Image:
    image = Image.new("RGB", size, (232, 235, 239))
    draw = ImageDraw.Draw(image)
    alternate = (204, 210, 217)
    for y in range(0, size[1], square):
        for x in range(0, size[0], square):
            if (x // square + y // square) % 2:
                draw.rectangle((x, y, x + square - 1, y + square - 1), fill=alternate)
    return image


def make_contact_sheet(paths: list[Path]) -> Path:
    labels = [
        "Sickle",
        "Wheat",
        "Corn",
        "Chicken Feed",
        "Empty Basket",
        "Bread",
    ]
    cell_size = (360, 360)
    label_height = 48
    margin = 24
    sheet_size = (
        margin * 2 + cell_size[0] * 3,
        margin * 2 + (cell_size[1] + label_height) * 2,
    )
    sheet = Image.new("RGB", sheet_size, (43, 48, 56))
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default(size=22)

    for index, (path, label) in enumerate(zip(paths, labels)):
        column = index % 3
        row = index // 3
        x = margin + column * cell_size[0]
        y = margin + row * (cell_size[1] + label_height)

        cell = checkerboard(cell_size)
        sprite = Image.open(path).convert("RGBA")
        preview = sprite.resize((cell_size[0], cell_size[1]), Image.Resampling.LANCZOS)
        cell.paste(preview, (0, 0), preview)
        sheet.paste(cell, (x, y))

        text_box = draw.textbbox((0, 0), label, font=font)
        text_width = text_box[2] - text_box[0]
        draw.text(
            (x + (cell_size[0] - text_width) // 2, y + cell_size[1] + 10),
            label,
            fill=(246, 248, 250),
            font=font,
        )

    destination = TEMP / "contact_sheet.png"
    sheet.save(destination)
    return destination


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    outputs: list[Path] = []

    for filename, (source_name, max_size) in SPRITES.items():
        destination = OUTPUT / filename
        normalize(TEMP / source_name, max_size).save(destination)
        bounds = validate(destination)
        print(f"{destination.relative_to(ROOT)} alpha_bounds={bounds}")
        outputs.append(destination)

    contact_sheet = make_contact_sheet(outputs)
    print(contact_sheet.relative_to(ROOT))


if __name__ == "__main__":
    main()
