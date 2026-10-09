import os
from PIL import Image

ARTIFACTS_DIR = r"C:\Users\Partridge\.gemini\antigravity-ide\brain\f53a3369-870c-476b-83cb-f20efa5fb55d"
PROJECT_ROOT = r"c:\Users\Partridge\Desktop\blood-seal"
TARGET_DIR = os.path.join(PROJECT_ROOT, "Assets", "Sprites", "Characters")

os.makedirs(TARGET_DIR, exist_ok=True)

def remove_white_background_floodfill(img_path, tolerance=25):
    img = Image.open(img_path).convert("RGBA")
    width, height = img.size
    pixels = img.load()

    visited = [[False] * height for _ in range(width)]
    queue = []

    def is_bg(x, y):
        r, g, b, a = pixels[x, y]
        return r >= (255 - tolerance) and g >= (255 - tolerance) and b >= (255 - tolerance)

    for x in range(width):
        if is_bg(x, 0): queue.append((x, 0)); visited[x][0] = True
        if is_bg(x, height - 1): queue.append((x, height - 1)); visited[x][height - 1] = True
    for y in range(height):
        if is_bg(0, y): queue.append((0, y)); visited[0][y] = True
        if is_bg(width - 1, y): queue.append((width - 1, y)); visited[width - 1][y] = True

    while queue:
        cx, cy = queue.pop(0)
        r, g, b, _ = pixels[cx, cy]
        pixels[cx, cy] = (r, g, b, 0)

        for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
            nx, ny = cx + dx, cy + dy
            if 0 <= nx < width and 0 <= ny < height and not visited[nx][ny]:
                visited[nx][ny] = True
                if is_bg(nx, ny):
                    queue.append((nx, ny))

    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)

    return img

def process_new_enemies():
    sprites = [
        ("skeleton_warrior_1791551120866.jpg", "enemy_skeleton_warrior.png", (220, 220)),
        ("crypt_revenant_1791551146647.jpg", "boss_crypt_revenant.png", (380, 380)),
        ("enemy_gargoyle_1791551168662.jpg", "enemy_gargoyle.png", (220, 220)),
        ("vampire_patriarch_1791551190703.jpg", "boss_vampire_patriarch.png", (380, 380)),
    ]

    for src_file, out_name, target_size in sprites:
        src_path = os.path.join(ARTIFACTS_DIR, src_file)
        out_path = os.path.join(TARGET_DIR, out_name)
        print(f"Normalizing {src_file} -> {out_path}...")
        img = remove_white_background_floodfill(src_path, tolerance=30)
        img.thumbnail(target_size, Image.Resampling.LANCZOS)
        img.save(out_path, "PNG")
        print(f"Saved {out_path} ({img.size})")

if __name__ == "__main__":
    process_new_enemies()
    print("New enemy sprites processed successfully!")
