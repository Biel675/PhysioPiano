import os
import re

# Map from flat to sharp notation
flat_to_sharp = {
    "Cb": "B",
    "Db": "Cs",
    "Eb": "Ds",
    "Fb": "E",
    "Gb": "Fs",
    "Ab": "Gs",
    "Bb": "As"
}

# folder containing MP3 files
folder = "./"   # change if needed

pattern = re.compile(r"([A-G][b])(\d)\.mp3$", re.IGNORECASE)

for filename in os.listdir(folder):
    match = pattern.match(filename)
    if match:
        flat_note = match.group(1)   # e.g. Db
        octave = match.group(2)      # e.g. 3

        # Convert to sharp notation
        sharp_note = flat_to_sharp.get(flat_note.capitalize())
        if not sharp_note:
            continue

        new_name = f"{sharp_note}{octave}.mp3"
        old_path = os.path.join(folder, filename)
        new_path = os.path.join(folder, new_name)

        print(f"Renaming {filename} → {new_name}")
        os.rename(old_path, new_path)
