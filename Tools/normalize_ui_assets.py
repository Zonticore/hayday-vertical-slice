from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
TEMP = ROOT / "Temp" / "ImageGen" / "UI"
OUTPUT = ROOT / "Assets" / "Sprites" / "UI"

ASSETS = {
    "ui_coin.png": ("coin_transparent.png", (512, 512), (448, 448)),
    "ui_xp_star.png": ("xp_star_transparent.png", (512, 512), (448, 448)),
    "ui_progress_bar_back.png": (
        "progress_back_transparent.png",
        (1024, 256),
        (960, 210),
    ),
    "ui_progress_bar_fill.png": (
        "progress_fill_transparent.png",
        (1024, 256),
        (960, 190),
    ),
    "ui_store_button.png": ("store_button_transparent.png", (512, 512), (448, 448)),
    "ui_store_panel.png": ("store_panel_transparent.png", (768, 1024), (710, 950)),
    "ui_context_menu_background.png": (
        "context_menu_transparent.png",
        (1024, 384),
        (960, 320),
    ),
}


def crop_subject(path: Path) -> Image.Image:
    image = Image.open(path).convert("RGBA")
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise ValueError(f"No visible pixels found in {path}")
    return image.crop(bounds)


def normalize(
    source: Path,
    canvas_size: tuple[int, int],
    max_size: tuple[int, int],
) -> Image.Image:
    subject = crop_subject(source)
    scale = min(max_size[0] / subject.width, max_size[1] / subject.height)
    size = (
        max(1, round(subject.width * scale)),
        max(1, round(subject.height * scale)),
    )
    subject = subject.resize(size, Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
    position = (
        (canvas_size[0] - size[0]) // 2,
        (canvas_size[1] - size[1]) // 2,
    )
    canvas.alpha_composite(subject, position)
    return canvas


def validate(path: Path, expected_size: tuple[int, int]) -> tuple[int, int, int, int]:
    image = Image.open(path).convert("RGBA")
    if image.size != expected_size:
        raise ValueError(f"Unexpected size for {path}: {image.size}")

    alpha = image.getchannel("A")
    corners = (
        alpha.getpixel((0, 0)),
        alpha.getpixel((image.width - 1, 0)),
        alpha.getpixel((0, image.height - 1)),
        alpha.getpixel((image.width - 1, image.height - 1)),
    )
    if corners != (0, 0, 0, 0):
        raise ValueError(f"Non-transparent corner in {path}: {corners}")

    bounds = alpha.getbbox()
    if bounds is None:
        raise ValueError(f"No visible sprite in {path}")
    return bounds


def checkerboard(size: tuple[int, int], square: int = 20) -> Image.Image:
    image = Image.new("RGB", size, (235, 238, 242))
    draw = ImageDraw.Draw(image)
    alternate = (205, 211, 219)
    for y in range(0, size[1], square):
        for x in range(0, size[0], square):
            if (x // square + y // square) % 2:
                draw.rectangle((x, y, x + square - 1, y + square - 1), fill=alternate)
    return image


def fit_preview(image: Image.Image, max_size: tuple[int, int]) -> Image.Image:
    scale = min(max_size[0] / image.width, max_size[1] / image.height)
    size = (max(1, round(image.width * scale)), max(1, round(image.height * scale)))
    return image.resize(size, Image.Resampling.LANCZOS)


def make_contact_sheet(paths: list[Path]) -> Path:
    labels = [
        "Coin",
        "XP Star",
        "Progress Back",
        "Progress Fill",
        "Store Button",
        "Store Panel",
        "Context Menu",
    ]
    cell_size = (420, 340)
    label_height = 44
    margin = 24
    columns = 3
    rows = 3
    sheet_size = (
        margin * 2 + cell_size[0] * columns,
        margin * 2 + (cell_size[1] + label_height) * rows,
    )
    sheet = Image.new("RGB", sheet_size, (43, 48, 56))
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default(size=21)

    for index, (path, label) in enumerate(zip(paths, labels)):
        column = index % columns
        row = index // columns
        x = margin + column * cell_size[0]
        y = margin + row * (cell_size[1] + label_height)
        cell = checkerboard(cell_size)

        sprite = Image.open(path).convert("RGBA")
        preview = fit_preview(sprite, (cell_size[0] - 24, cell_size[1] - 24))
        preview_x = (cell_size[0] - preview.width) // 2
        preview_y = (cell_size[1] - preview.height) // 2
        cell.paste(preview, (preview_x, preview_y), preview)
        sheet.paste(cell, (x, y))

        text_box = draw.textbbox((0, 0), label, font=font)
        text_width = text_box[2] - text_box[0]
        draw.text(
            (x + (cell_size[0] - text_width) // 2, y + cell_size[1] + 9),
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

    for filename, (source_name, canvas_size, max_size) in ASSETS.items():
        destination = OUTPUT / filename
        normalize(TEMP / source_name, canvas_size, max_size).save(destination)
        bounds = validate(destination, canvas_size)
        print(f"{destination.relative_to(ROOT)} alpha_bounds={bounds}")
        outputs.append(destination)

    contact_sheet = make_contact_sheet(outputs)
    print(contact_sheet.relative_to(ROOT))


if __name__ == "__main__":
    main()
