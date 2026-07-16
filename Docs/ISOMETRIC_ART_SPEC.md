# Isometric Art Specification

## Master canvas

- Export every sprite as a 1024 x 1024 px PNG in sRGB.
- Every final sprite must have a transparent background and an alpha channel.
- Pixels outside the requested sprite must have alpha 0. Do not retain a white, black, colored, checkerboard, or scenery background.
- Use a fixed orthographic 2:1 isometric projection with no perspective distortion.
- Use a 256 x 128 px logical tile.
- Use ground anchor `(512, 768)` and normalized pivot `(0.5, 0.75)`.
- Keep at least 48 px of transparent padding around visible artwork.

## Tile alignment

A 1 x 1 tile is centered on `(512, 768)` with these vertices:

- Top: `(512, 704)`
- Right: `(640, 768)`
- Bottom: `(512, 832)`
- Left: `(384, 768)`

The grid axes remain at approximately plus and minus 26.565 degrees. The primary view shows the front-left and front-right faces. Vertical edges remain vertical.

## Footprints

| Footprint | Ground bounds | Typical use |
| --- | --- | --- |
| 1 x 1 | 256 x 128 px | Crops, animals, tiles, small props |
| 2 x 2 | 512 x 256 px | Feed mill, bakery, silo |
| 3 x 3 | 768 x 384 px | Barn, large animal pen |

Keep the footprint centered on the master ground anchor. Buildings may rise upward, but their ground anchor, scale, and footprint must not shift between states.

## Direction, lighting, and scale

- Back-left points upper-left; back-right points upper-right.
- Front-left points lower-left; front-right points lower-right.
- Use soft, even daylight from the upper-left.
- Shadows fall toward the lower-right.
- Avoid cinematic lighting, depth of field, lens distortion, and hard scene shadows.
- Objects should occupy approximately 80-90% of their stated footprint unless a smaller scale is specified.
- Doors, characters, tools, crops, and props must retain the same apparent scale across the set.

## State consistency

All states and animation frames of an object must use the same canvas, pivot, footprint, projection, camera, lighting, and scale. Do not shift, crop, rotate, or resize an object between frames.

## Reusable generation prompt

```text
Create a clean isometric mobile farming-game sprite of [SUBJECT].

STATE:
[STATE OR ANIMATION FRAME]

FOOTPRINT:
[1x1, 2x2, 3x3, or WIDTH x DEPTH] isometric tiles.

STRICT TECHNICAL SPECIFICATION:
Use a fixed orthographic 2:1 isometric projection with no perspective distortion.
The final image must be exactly 1024x1024 pixels and exported as a PNG with a
fully transparent background and a valid alpha channel. Every pixel outside the
requested sprite must have alpha 0. Do not include a white, black, colored,
checkerboard, scenic, or photographic background.
One isometric tile is exactly 256 pixels wide and 128 pixels tall.
Center the object's ground footprint at pixel coordinate 512,768.
Treat coordinate 512,768 as the permanent ground anchor and pivot.
The object must rise vertically upward from this anchor.
Show the front-left and front-right faces.
The isometric horizontal axes remain at approximately plus and minus 26.565 degrees.
Do not rotate, resize, shift, crop, or change the camera.
Maintain at least 48 pixels of transparent padding around the visible artwork.

LIGHTING:
Use soft, consistent lighting from the upper-left.
Use a subtle contact shadow falling toward the lower-right when appropriate.
Avoid dramatic lighting, perspective, depth of field, or environmental background.

SCALE:
The object occupies approximately 85 percent of its specified ground footprint
while remaining consistent with other assets generated from this specification.

STYLE:
Friendly stylized 3D mobile farming-game artwork, rounded readable shapes,
slightly exaggerated proportions, warm farm colors, hand-painted texture,
moderate detail, clean edges, and soft shading.

The sprite contains only the requested subject and its optional contact shadow.
No interface elements, labels, text, scenery, border, platform, or decorative frame.
```

## Negative prompt

```text
opaque background, white background, black background, colored background,
checkerboard background, perspective camera, three-point perspective,
top-down view, side view, front view, camera tilt, camera rotation,
different isometric angle, fisheye, lens distortion, horizon, landscape,
floating island, circular platform, square platform, pedestal, UI, text,
label, border, frame, cropped object, off-center object, inconsistent scale,
oversized object, tiny object, hard cinematic shadow, dramatic lighting,
depth of field, motion blur, watermark
```

## Export validation

Before accepting a sprite, verify:

1. The file is a 1024 x 1024 PNG in RGBA mode.
2. All four canvas corners have alpha 0.
3. The ground anchor is `(512, 768)`.
4. The footprint matches the intended tile guide.
5. The asset is not cropped and has no background fringe.
6. Related states do not shift or change scale.

