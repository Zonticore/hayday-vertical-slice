from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
TEMP = ROOT / "Temp" / "ImageGen" / "FarmPatch"
OUTPUT = ROOT / "Assets" / "Art" / "Isometric" / "FarmPatch"
CANVAS_SIZE = (1024, 1024)
ANCHOR = (512, 768)
TILE_SIZE = (256, 128)
TILE_ORIGIN = (384, 704)


def crop_subject(path: Path) -> Image.Image:
    image = Image.open(path).convert("RGBA")
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise ValueError(f"No visible pixels found in {path}")
    return image.crop(bounds)


def diamond_mask(size: tuple[int, int]) -> Image.Image:
    width, height = size
    scale = 4
    mask = Image.new("L", (width * scale, height * scale), 0)
    draw = ImageDraw.Draw(mask)
    draw.polygon(
        [
            (width * scale // 2, 0),
            (width * scale - 1, height * scale // 2),
            (width * scale // 2, height * scale - 1),
            (0, height * scale // 2),
        ],
        fill=255,
    )
    return mask.resize(size, Image.Resampling.LANCZOS)


def empty_canvas() -> Image.Image:
    return Image.new("RGBA", CANVAS_SIZE, (0, 0, 0, 0))


def normalize_patch_base(source: Path) -> Image.Image:
    subject = crop_subject(source)
    center_color = subject.getpixel((subject.width // 2, subject.height // 2))[:3]
    flattened = Image.new("RGBA", subject.size, center_color + (255,))
    flattened.alpha_composite(subject)
    tile = flattened.resize(TILE_SIZE, Image.Resampling.LANCZOS)
    tile.putalpha(diamond_mask(TILE_SIZE))

    canvas = empty_canvas()
    canvas.alpha_composite(tile, TILE_ORIGIN)
    return canvas


def normalize_exact_overlay(
    source: Path,
    size: tuple[int, int],
    bottom: int,
) -> Image.Image:
    subject = crop_subject(source).resize(size, Image.Resampling.LANCZOS)
    canvas = empty_canvas()
    xy = (ANCHOR[0] - size[0] // 2, bottom - size[1])
    canvas.alpha_composite(subject, xy)
    return canvas


def normalize_proportional_overlay(
    source: Path,
    max_size: tuple[int, int],
    bottom: int,
) -> Image.Image:
    subject = crop_subject(source)
    scale = min(max_size[0] / subject.width, max_size[1] / subject.height)
    size = (max(1, round(subject.width * scale)), max(1, round(subject.height * scale)))
    subject = subject.resize(size, Image.Resampling.LANCZOS)
    canvas = empty_canvas()
    xy = (ANCHOR[0] - size[0] // 2, bottom - size[1])
    canvas.alpha_composite(subject, xy)
    return canvas


def combine(base: Image.Image, overlay: Image.Image) -> Image.Image:
    result = base.copy()
    result.alpha_composite(overlay)
    return result


def validate(path: Path) -> None:
    image = Image.open(path).convert("RGBA")
    if image.size != CANVAS_SIZE:
        raise ValueError(f"Unexpected size for {path}: {image.size}")

    alpha = image.getchannel("A")
    corners = [
        alpha.getpixel((0, 0)),
        alpha.getpixel((1023, 0)),
        alpha.getpixel((0, 1023)),
        alpha.getpixel((1023, 1023)),
    ]

    if corners != [0, 0, 0, 0]:
        raise ValueError(f"Non-transparent corner in {path}: {corners}")
    if alpha.getbbox() is None:
        raise ValueError(f"No visible sprite in {path}")


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)

    base = normalize_patch_base(TEMP / "farm_patch_empty_raw.png")
    selected = normalize_exact_overlay(
        TEMP / "farm_patch_selected_raw.png",
        (272, 144),
        840,
    )

    crop_specs = {
        "farm_patch_wheat_planted.png": ("wheat_planted_raw.png", (210, 78), 816),
        "farm_patch_wheat_early.png": ("wheat_early_raw.png", (220, 135), 818),
        "farm_patch_wheat_middle.png": ("wheat_middle_raw.png", (230, 155), 820),
        "farm_patch_wheat_mature.png": ("wheat_mature_raw.png", (240, 190), 820),
        "farm_patch_wheat_cutting.png": ("wheat_cutting_raw.png", (240, 185), 820),
        "farm_patch_wheat_harvested.png": ("wheat_harvested_raw.png", (220, 95), 818),
    }

    outputs = [
        OUTPUT / "farm_patch_empty.png",
        OUTPUT / "farm_patch_selected_overlay.png",
    ]

    base.save(outputs[0])
    selected.save(outputs[1])

    for filename, (source_name, size, bottom) in crop_specs.items():
        overlay = normalize_exact_overlay(TEMP / source_name, size, bottom)
        destination = OUTPUT / filename
        combine(base, overlay).save(destination)
        outputs.append(destination)

    bundle = normalize_proportional_overlay(
        TEMP / "wheat_bundle_raw.png",
        (160, 110),
        826,
    )
    bundle_path = OUTPUT / "wheat_bundle_ground.png"
    bundle.save(bundle_path)
    outputs.append(bundle_path)

    collection = normalize_proportional_overlay(
        TEMP / "wheat_collection_effect_raw.png",
        (210, 220),
        830,
    )
    collection_path = OUTPUT / "wheat_collection_effect.png"
    collection.save(collection_path)
    outputs.append(collection_path)

    for output in outputs:
        validate(output)
        print(output.relative_to(ROOT))


if __name__ == "__main__":
    main()
