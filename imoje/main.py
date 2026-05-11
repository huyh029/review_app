import re
import json

# Đọc file fulltext.txt
with open('fulltext.txt', 'r', encoding='utf-8') as f:
    content = f.read()

# Tìm tất cả category và emoji của nó
# Pattern: <div id="f\d+" class="category"...> <span>Category Name</span> ... </div>
categories = {}

# Tìm tất cả div category
category_pattern = r'<div id="(f\d+)"[^>]*>.*?<span>([^<]+)</span>(.*?)</div>'
category_matches = re.finditer(category_pattern, content, re.DOTALL)

for cat_match in category_matches:
    cat_id = cat_match.group(1)
    cat_name = cat_match.group(2)
    cat_content = cat_match.group(3)
    
    # Tìm tất cả emoji trong category này
    emoji_pattern = r'<p[^>]*>([^<]+)</p>'
    emoji_matches = re.findall(emoji_pattern, cat_content)
    
    emojis = []
    for emoji in emoji_matches:
        emoji = emoji.strip()
        # Kiểm tra xem có phải emoji không
        if emoji and len(emoji) <= 10 and any(ord(c) > 127 for c in emoji):
            emojis.append(emoji)
    
    if emojis:
        # Loại bỏ duplicate
        emojis = list(dict.fromkeys(emojis))
        categories[cat_name] = emojis

# Ghi vào file emoji.txt dưới dạng JSON
with open('emoji.txt', 'w', encoding='utf-8') as f:
    json.dump(categories, f, ensure_ascii=False, indent=2)

# In thống kê
total_emojis = sum(len(emojis) for emojis in categories.values())
print(f"Tìm thấy {len(categories)} category")
print(f"Tổng cộng {total_emojis} emoji")
for cat_name, emojis in categories.items():
    print(f"  - {cat_name}: {len(emojis)} emoji")
print("Đã lưu vào emoji.txt")
