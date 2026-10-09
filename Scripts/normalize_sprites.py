import os
from PIL import Image, ImageFilter

ARTIFACTS_DIR = r"C:\Users\Partridge\.gemini\antigravity-ide\brain\f53a3369-870c-476b-83cb-f20efa5fb55d"
PROJECT_ROOT = r"c:\Users\Partridge\Desktop\blood-seal"

TARGET_DIRS = {
    "Characters": os.path.join(PROJECT_ROOT, "Assets", "Sprites", "Characters"),
    "Pet": os.path.join(PROJECT_ROOT, "Assets", "Sprites", "Pet"),
    "Background": os.path.join(PROJECT_ROOT, "Assets", "Sprites", "Background"),
}

for d in TARGET_DIRS.values():
    os.makedirs(d, exist_ok=True)

def remove_white_background_floodfill(img_path, tolerance=25):
    """
    Flood fills from the 4 corners/edges to remove only outer white/light background,
    protecting internal white/bone elements like skull masks and teeth.
    """
    img = Image.open(img_path).convert("RGBA")
    width, height = img.size
    pixels = img.load()

    # Visited grid
    visited = [[False] * height for _ in range(width)]
    queue = []

    def is_bg(x, y):
        r, g, b, a = pixels[x, y]
        # Background is close to white (typically > 240)
        return r >= (255 - tolerance) and g >= (255 - tolerance) and b >= (255 - tolerance)

    # Seed all border pixels
    for x in range(width):
        if is_bg(x, 0): queue.append((x, 0)); visited[x][0] = True
        if is_bg(x, height - 1): queue.append((x, height - 1)); visited[x][height - 1] = True
    for y in range(height):
        if is_bg(0, y): queue.append((0, y)); visited[0][y] = True
        if is_bg(width - 1, y): queue.append((width - 1, y)); visited[width - 1][y] = True

    # 4-directional BFS
    while queue:
        cx, cy = queue.pop(0)
        # Set alpha to 0 for background
        r, g, b, _ = pixels[cx, cy]
        pixels[cx, cy] = (r, g, b, 0)

        for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
            nx, ny = cx + dx, cy + dy
            if 0 <= nx < width and 0 <= ny < height and not visited[nx][ny]:
                visited[nx][ny] = True
                if is_bg(nx, ny):
                    queue.append((nx, ny))

    # Clean edges with soft feathering on boundary
    # Bounding box crop
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)

    return img

def process_character_sprites():
    sprites = [
        ("hero_knight_1791540333018.jpg", "Characters", "hero_knight.png", (256, 256)),
        ("enemy_cultist_1791540349421.jpg", "Characters", "enemy_cultist.png", (220, 220)),
        ("boss_abomination_1791540423131.jpg", "Characters", "boss_abomination.png", (380, 380)),
        ("pet_blood_raven_1791540440866.jpg", "Pet", "pet_blood_raven.png", (140, 140)),
    ]

    for src_file, category, out_name, target_size in sprites:
        src_path = os.path.join(ARTIFACTS_DIR, src_file)
        out_path = os.path.join(TARGET_DIRS[category], out_name)
        print(f"Processing {src_file} -> {out_path}...")
        img = remove_white_background_floodfill(src_path, tolerance=30)
        
        # Resize maintaining aspect ratio
        img.thumbnail(target_size, Image.Resampling.LANCZOS)
        img.save(out_path, "PNG")
        print(f"Saved {out_path} ({img.size})")

def process_backgrounds():
    bgs = [
        ("bg_sky_bloodmoon_1791540458219.jpg", "bg_sky_bloodmoon.png", (1920, 1080), False),
        ("bg_ruins_spires_1791540477093.jpg", "bg_ruins_spires.png", (1920, 1080), True),
        ("bg_ground_cobble_1791540501586.jpg", "bg_ground_cobble.png", (1920, 1080), True),
    ]

    for src_file, out_name, target_size, add_alpha_gradient in bgs:
        src_path = os.path.join(ARTIFACTS_DIR, src_file)
        out_path = os.path.join(TARGET_DIRS["Background"], out_name)
        print(f"Processing background {src_file} -> {out_path}...")
        img = Image.open(src_path).convert("RGBA")
        img = img.resize(target_size, Image.Resampling.LANCZOS)

        if add_alpha_gradient:
            # For ruins, fade out dark sky at the top so Blood Moon behind it shines through!
            width, height = img.size
            pixels = img.load()
            if "spires" in out_name:
                # Top 35% fades out from 0 to 255 alpha
                fade_end = int(height * 0.40)
                for y in range(height):
                    for x in range(width):
                        r, g, b, a = pixels[x, y]
                        # Top sky is very dark; if y < fade_end, make alpha fade out
                        if y < fade_end:
                            factor = y / float(fade_end)
                            # Also consider dark pixels
                            alpha = int(255 * factor)
                            pixels[x, y] = (r, g, b, min(a, alpha))
            elif "ground" in out_name:
                # Top 40% fades out so ruins behind tombstones are visible
                fade_end = int(height * 0.45)
                for y in range(height):
                    for x in range(width):
                        r, g, b, a = pixels[x, y]
                        if y < fade_end:
                            factor = y / float(fade_end)
                            alpha = int(255 * factor)
                            pixels[x, y] = (r, g, b, min(a, alpha))

        img.save(out_path, "PNG")
        print(f"Saved background {out_path} ({img.size})")

if __name__ == "__main__":
    process_character_sprites()
    process_backgrounds()
    print("All sprites processed and saved successfully!")
