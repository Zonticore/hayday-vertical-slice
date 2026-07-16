from pathlib import Path

from PIL import Image, ImageChops, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
TEMP = ROOT / "Temp" / "ImageGen" / "TerrainPlacement"
ISO = ROOT / "Assets" / "Art" / "Isometric"
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


def diamond_mask(size: tuple[int, int], opacity: int = 255) -> Image.Image:
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
        fill=opacity,
    )
    return mask.resize(size, Image.Resampling.LANCZOS)


def place_on_canvas(sprite: Image.Image, xy: tuple[int, int]) -> Image.Image:
    canvas = Image.new("RGBA", CANVAS_SIZE, (0, 0, 0, 0))
    canvas.alpha_composite(sprite, xy)
    return canvas


def normalize_tile_texture(source: Path, destination: Path) -> None:
    subject = crop_subject(source)
    center_color = subject.getpixel((subject.width // 2, subject.height // 2))[:3]
    flattened = Image.new("RGBA", subject.size, center_color + (255,))
    flattened.alpha_composite(subject)
    tile = flattened.resize(TILE_SIZE, Image.Resampling.LANCZOS)
    tile.putalpha(diamond_mask(TILE_SIZE))
    destination.parent.mkdir(parents=True, exist_ok=True)
    place_on_canvas(tile, TILE_ORIGIN).save(destination)


def normalize_overlay(source: Path, destination: Path, alpha_scale: float = 1.0) -> None:
    tile = crop_subject(source).resize(TILE_SIZE, Image.Resampling.LANCZOS)
    source_alpha = tile.getchannel("A")
    shape = diamond_mask(TILE_SIZE)
    scaled_alpha = source_alpha.point(lambda value: round(value * alpha_scale))
    alpha = ImageChops.multiply(scaled_alpha, shape)
    tile.putalpha(alpha)
    destination.parent.mkdir(parents=True, exist_ok=True)
    place_on_canvas(tile, TILE_ORIGIN).save(destination)


def normalize_shadow(
    source: Path,
    destination: Path,
    size: tuple[int, int],
    max_alpha: int,
) -> None:
    shadow = crop_subject(source).resize(size, Image.Resampling.LANCZOS)
    source_alpha = shadow.getchannel("A")
    source_max = max(1, source_alpha.getextrema()[1])
    normalized_alpha = source_alpha.point(lambda value: round(value * max_alpha / source_max))
    neutral = Image.new("RGBA", size, (24, 28, 25, 0))
    neutral.putalpha(normalized_alpha)
    xy = (ANCHOR[0] - size[0] // 2, ANCHOR[1] - size[1] // 2)
    destination.parent.mkdir(parents=True, exist_ok=True)
    place_on_canvas(neutral, xy).save(destination)


def normalize_object(
    source: Path,
    destination: Path,
    max_size: tuple[int, int],
    bottom: int,
) -> None:
    sprite = crop_subject(source)
    scale = min(max_size[0] / sprite.width, max_size[1] / sprite.height)
    size = (max(1, round(sprite.width * scale)), max(1, round(sprite.height * scale)))
    sprite = sprite.resize(size, Image.Resampling.LANCZOS)
    xy = (ANCHOR[0] - sprite.width // 2, bottom - sprite.height)
    destination.parent.mkdir(parents=True, exist_ok=True)
    place_on_canvas(sprite, xy).save(destination)


def validate(path: Path) -> None:
    image = Image.open(path).convert("RGBA")
    if image.size != CANVAS_SIZE:
        raise ValueError(f"Unexpected size for {path}: {image.size}")
    alpha = image.getchannel("A")
    corners = [alpha.getpixel((0, 0)), alpha.getpixel((1023, 0)), alpha.getpixel((0, 1023)), alpha.getpixel((1023, 1023))]
    if corners != [0, 0, 0, 0]:
        raise ValueError(f"Non-transparent canvas corner in {path}: {corners}")
    if alpha.getbbox() is None:
        raise ValueError(f"No visible sprite in {path}")


def main() -> None:
    outputs = [
        ISO / "Tiles" / "grass_ground_tile.png",
        ISO / "Tiles" / "dirt_path_tile.png",
        ISO / "Placement" / "selection_tile.png",
        ISO / "Placement" / "valid_placement_tile.png",
        ISO / "Placement" / "invalid_placement_tile.png",
        ISO / "Shadows" / "building_shadow_2x2.png",
        ISO / "Shadows" / "small_object_shadow_1x1.png",
        ISO / "Fences" / "fence_straight.png",
        ISO / "Fences" / "fence_corner.png",
        ISO / "Fences" / "fence_gate.png",
    ]

    normalize_tile_texture(
        ISO / "Tiles" / "grass_ground_tile_raw.png",
        outputs[0],
    )
    normalize_tile_texture(TEMP / "dirt_path_raw.png", outputs[1])
    normalize_overlay(TEMP / "selection_tile_raw.png", outputs[2])
    normalize_overlay(TEMP / "valid_placement_raw.png", outputs[3], alpha_scale=0.68)
    normalize_overlay(TEMP / "invalid_placement_raw.png", outputs[4], alpha_scale=0.68)
    normalize_shadow(TEMP / "building_shadow_raw.png", outputs[5], (384, 160), 135)
    normalize_shadow(TEMP / "small_shadow_raw.png", outputs[6], (144, 56), 120)
    normalize_object(TEMP / "fence_straight_raw.png", outputs[7], (300, 260), 848)
    normalize_object(TEMP / "fence_corner_raw.png", outputs[8], (340, 240), 850)
    normalize_object(TEMP / "fence_gate_raw.png", outputs[9], (300, 260), 848)

    for output in outputs:
        validate(output)
        print(output.relative_to(ROOT))


if __name__ == "__main__":
    main()
